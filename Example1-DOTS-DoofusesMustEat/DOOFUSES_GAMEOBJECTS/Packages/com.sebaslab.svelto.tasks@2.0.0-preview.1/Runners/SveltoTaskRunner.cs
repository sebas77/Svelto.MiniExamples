using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.Tasks.DataStructures;

namespace Svelto.Tasks.Internal
{
    public static class SveltoTaskRunner<T>  where T : ISveltoTask
    {
        internal class Process<TFlowModifier> : IProcessSveltoTasks where TFlowModifier: IFlowModifier
        {
            public override string ToString()
            {
                return _info.runnerName;
            }

            public Process
            (ThreadSafeQueue<T> newTaskRoutines, FasterList<T> coroutines, FlushingOperation flushingOperation
           , TFlowModifier info)
            {
                _newTaskRoutines   = newTaskRoutines;
                _coroutines        = coroutines;
                _flushingOperation = flushingOperation;
                _info              = info;
            }    

            public bool MoveNext<PlatformProfiler>(in PlatformProfiler platformProfiler) 
                where PlatformProfiler : IPlatformProfiler
            {
                 DBC.Tasks.Check.Require(_flushingOperation.paused == false
                   || _flushingOperation.kill == false
                    , $"cannot be found in pause state if killing has been initiated {_info.runnerName}");
                 DBC.Tasks.Check.Require(_flushingOperation.kill == false 
                                      || _flushingOperation.stopping == true
                                       , $"if a runner is killed, must be stopped {_info.runnerName}");

                 if (_flushingOperation.flush)
                 {
                     _newTaskRoutines.Clear();
                 }
                 
                 //a stopped runner can restart and the design allows to queue new tasks in the stopped state
                 //although they won't be processed. In this sense it's similar to paused. For this reason
                 //_newTaskRoutines cannot be cleared in paused and stopped state.
                 //This is done before the stopping check because all the tasks queued before stop will be stopped
                 if (_newTaskRoutines.count > 0 && _flushingOperation.acceptsNewTasks == true)
                 {
                     _newTaskRoutines.DequeueAllInto(_coroutines);
                 }
                 
                //the difference between stop and pause is that pause freeze the tasks states, while stop flush
                //them until there is nothing to run. Ever looping tasks are forced to be stopped and therefore
                //can terminate naturally
                if (_flushingOperation.stopping == true)
                {
                    //remember: it is not possible to clear new tasks after a runner is stopped, because a runner
                    //doesn't react immediately to a stop, so new valid tasks after the stop may be queued meanwhile.
                    //A Flush should be the safe way to be sure that only the tasks in process up to the Stop()
                    //point are stopped.
                    if (_coroutines.count == 0)
                    {
                        if (_flushingOperation.kill == true)
                        {
                            //todo what happens to the continuationenumerators?
                            _coroutines.Clear();
                            _newTaskRoutines.Clear();
                            return false;
                        }

                        //once all the coroutines are flushed the loop can return accepting new tasks
                        _flushingOperation.Unstop();
                    }
                }

                var coroutinesCount = _coroutines.count;
                
                if (coroutinesCount == 0 || (_flushingOperation.paused == true && _flushingOperation.stopping == false))
                {
                    return true;
                }
                
#if TASKS_PROFILER_ENABLED
                Profiler.TaskProfiler.ResetDurations(_info.runnerName);
#endif
                _info.Reset();

                //Note: old comment, left as memo, when I used to allow to run tasks immediately
                //I decided to adopt this strategy instead to call MoveNext() directly when a task
                //must be executed immediately. However this works only if I do not update the coroutines count
                //after the MoveNext which on its turn could run immediately another task.
                //this can cause a stack of MoveNext, which works only because I add the task to run immediately
                //at the end of the list and the child MoveNext executes only the new one. When the stack
                //goes back to the previous MoveNext, I don't want to execute the new just added task again,
                //so I can't update the coroutines count, it must stay the previous one/
                int index = 0;
                bool mustExit;

                var coroutines = _coroutines.ToArrayFast(out _);

                do
                {
                    if (_info.CanProcessThis(ref index) == false) break;

                    bool result;

                    if (_flushingOperation.stopping) coroutines[index].Stop();

#if ENABLE_PLATFORM_PROFILER
                    using (platformProfiler.Sample(coroutines[index].name))
#endif
#if TASKS_PROFILER_ENABLED
                        result =
                            Profiler.TaskProfiler.MonitorUpdateDuration(ref coroutines[index], _info.runnerName);
#else
                        result = coroutines[index].MoveNext();
#endif
                    //MoveNext may now cause tasks to run immediately and therefore increase the array size
                    //this side effect is due to the fact that I don't have a stack for each task anymore
                    //like I used to do in Svelto tasks 1.5 and therefore running new enumerators would
                    //mean to add new coroutines. However I do not want to iterate over the new coroutines
                    //during this iteration, so I won't modify coroutinesCount avoid this complexity disabling run
                    //immediate
                    //coroutines = _coroutines.ToArrayFast(out _);

                    int previousIndex = index;

                    if (result == false)
                    {
                        _coroutines.UnorderedRemoveAt((uint) index);

                        coroutinesCount--;
                    }
                    else
                        index++;

                    mustExit = (coroutinesCount == 0 || 
                                _info.CanMoveNext(ref index, ref coroutines[previousIndex], coroutinesCount) ==
                                false ||
                                index >= coroutinesCount);
                } while (!mustExit);

                return true;
            }
            
            readonly ThreadSafeQueue<T> _newTaskRoutines;
            readonly FasterList<T>      _coroutines;
            readonly FlushingOperation  _flushingOperation;
            
            TFlowModifier _info;
        }
        
        //todo this must copy the SveltoTaskState pattern
        public class FlushingOperation
        {
            public bool paused          => Volatile.Read(ref _paused);
            public bool stopping        => Volatile.Read(ref _stopped);
            public bool kill            => Volatile.Read(ref _killed);
            public bool flush            => Volatile.Read(ref _flush);
            public bool acceptsNewTasks => paused == false && stopping == false && kill == false;  
            
            public void Stop(string name)
            {
                DBC.Tasks.Check.Require(kill == false, $"cannot stop a runner that is killed {name}");
                
                //todo: careful if I intended these operations to be atomic, they are not!
                //maybe I want both flags to be set in a thread safe way This must be bitmask
                Volatile.Write(ref _stopped, true);
                Volatile.Write(ref _paused, false);
            }

            public void StopAndFlush()
            {
                Volatile.Write(ref _flush, true);
                Volatile.Write(ref _stopped, true);
                Volatile.Write(ref _paused, false);
            }

            public void Kill(string name)
            {
                DBC.Tasks.Check.Require(kill == false, $"cannot kill a runner that is killed {name}");
                
                //todo: careful if I intended these operations to be atomic, they are not!
                //maybe I want both flags to be set in a thread safe way, meaning that the
                //flags must all be set at once. This must be bitmask
                Volatile.Write(ref _stopped, true);
                Volatile.Write(ref _killed, true);
                Volatile.Write(ref _paused, false);
            }

            public void Pause(string name)
            {
                DBC.Tasks.Check.Require(kill == false, $"cannot pause a runner that is killed {name}");

                Volatile.Write(ref _paused, true);
            }

            public void Resume(string name)
            {
                DBC.Tasks.Check.Require(kill == false, $"cannot resume a runner that is killed {name}");
                
                Volatile.Write(ref _paused, false);
            }

            internal void Unstop()
            {
                Volatile.Write(ref _stopped, false);
            }
            
            bool _paused;
            bool _stopped;
            bool _killed;
            bool _flush;
        }
    }
}