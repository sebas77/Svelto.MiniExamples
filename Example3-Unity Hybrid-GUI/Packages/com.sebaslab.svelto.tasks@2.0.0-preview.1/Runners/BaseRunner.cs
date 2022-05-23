using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.Tasks.DataStructures;
using Svelto.Tasks.Internal;

namespace Svelto.Tasks
{
    /// <summary>
    /// Remember, unless you are using the StandardSchedulers, nothing hold your runners. Be careful that if you
    /// don't hold a reference, they will be garbage collected even if tasks are still running
    /// </summary>
    public abstract class BaseRunner<T> : IRunner, IRunner<T> where T : ISveltoTask
    {
        public bool isStopping => _flushingOperation.stopping;

//        public bool isKilled   => _flushingOperation.kill;
        public bool hasTasks => numberOfProcessingTasks != 0;

        public uint numberOfRunningTasks    => (uint) _coroutines.count;
        public uint numberOfQueuedTasks     => _newTaskRoutines.count;
        public uint numberOfProcessingTasks => (uint) (_newTaskRoutines.count + _coroutines.count);

        protected BaseRunner(string name, int size)
        {
            _name            = name;
            _newTaskRoutines = new ThreadSafeQueue<T>(size);
            _coroutines      = new FasterList<T>((uint) size);
        }

        protected BaseRunner(string name)
        {
            _name            = name;
            _newTaskRoutines = new ThreadSafeQueue<T>(NUMBER_OF_INITIAL_COROUTINE);
            _coroutines      = new FasterList<T>(NUMBER_OF_INITIAL_COROUTINE);
        }

        ~BaseRunner()
        {
            Console.LogWarning(_name.FastConcat(" has been garbage collected, this could have serious"
                                              + "consequences, are you sure you want this? "));

            _flushingOperation.Kill(_name);
        }

        public void Flush() { StopAndFlush(); }

        void StopAndFlush() { _flushingOperation.StopAndFlush(); }

        public void Pause() { _flushingOperation.Pause(_name); }

        public void Resume() { _flushingOperation.Resume(_name); }

        public void Step()
        {
            using (var platform = new PlatformProfiler(_name))
            {
                _processEnumerator.MoveNext(platform);
            }
        }

        /// <summary>
        /// TaskRunner doesn't stop executing tasks between scenes it's the final user responsibility to stop the tasks
        /// if needed
        /// </summary>
        public virtual void Stop()
        {
            //even if there are 0 coroutines, this must marked as stopping as during the stopping phase I don't want
            //new task to be put in the processing queue. So in the situation of 0 processing tasks but N 
            //waiting tasks, the waiting tasks must stay in the waiting list
            _flushingOperation.Stop(_name);
        }

        public void StartCoroutine(in T task)
        {
            DBC.Tasks.Check.Require(_flushingOperation.kill == false, $"can't schedule new routines on a killed scheduler {_name}");
            
            _newTaskRoutines.Enqueue(task);
        }

        public virtual void Dispose()
        {
            if (_flushingOperation.kill == true)
            {
                Console.LogDebugWarning($"disposing an already disposed runner?! {_name}");

                return;
            }

            _flushingOperation.Kill(_name);

            GC.SuppressFinalize(this);
        }

        protected IProcessSveltoTasks InitializeRunner<TFlowModified>
            (TFlowModified modifier) where TFlowModified : IFlowModifier
        {
            _processEnumerator =
                new SveltoTaskRunner<T>.Process<TFlowModified>(_newTaskRoutines, _coroutines, _flushingOperation
                                                             , modifier);

            return _processEnumerator;
        }

        IProcessSveltoTasks                            _processEnumerator;
        readonly ThreadSafeQueue<T>                    _newTaskRoutines;
        readonly FasterList<T>                         _coroutines;
        readonly SveltoTaskRunner<T>.FlushingOperation _flushingOperation = new SveltoTaskRunner<T>.FlushingOperation();

        readonly string _name;

        const int NUMBER_OF_INITIAL_COROUTINE = 3;
    }
}