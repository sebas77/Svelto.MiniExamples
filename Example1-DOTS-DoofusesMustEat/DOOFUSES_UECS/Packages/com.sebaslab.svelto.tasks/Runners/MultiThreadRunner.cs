using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.Tasks.DataStructures;
using Svelto.Tasks.FlowModifiers;
using Svelto.Tasks.Internal;
using Svelto.Utilities;

#if NETFX_CORE
using System.Threading.Tasks;
#endif

namespace Svelto.Tasks
{
    namespace Lean
    {
        public sealed class MultiThreadRunner : MultiThreadRunner<IEnumerator<TaskContract>>
        {
            public MultiThreadRunner
                (string name, bool relaxed = false, bool tightTasks = false) : base(name, relaxed, tightTasks) { }

            public MultiThreadRunner(string name, float intervalInMs) : base(name, intervalInMs) { }
        }

        public class MultiThreadRunner<T> : Svelto.Tasks.MultiThreadRunner<LeanSveltoTask<T>>
            where T : IEnumerator<TaskContract>
        {
            public MultiThreadRunner
                (string name, bool relaxed = false, bool tightTasks = false) : base(name, relaxed, tightTasks) { }

            public MultiThreadRunner(string name, float intervalInMs) : base(name, intervalInMs) { }
        }
    }

    namespace ExtraLean
    {
        public sealed class MultiThreadRunner : MultiThreadRunner<IEnumerator>
        {
            public MultiThreadRunner
                (string name, bool relaxed = false, bool tightTasks = false) : base(name, relaxed, tightTasks) { }

            public MultiThreadRunner(string name, float intervalInMs) : base(name, intervalInMs) { }
        }

        public class MultiThreadRunner<T> : Svelto.Tasks.MultiThreadRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public MultiThreadRunner
                (string name, bool relaxed = false, bool tightTasks = false) : base(name, relaxed, tightTasks) { }

            public MultiThreadRunner(string name, float intervalInMs) : base(name, intervalInMs) { }
        }
    }

    public class MultiThreadRunner<TTask> : MultiThreadRunner<TTask, StandardRunningInfo> where TTask : ISveltoTask
    {
        public MultiThreadRunner
            (string name, bool relaxed = false, bool tightTasks = false) : base(
            name, new StandardRunningInfo(), relaxed, tightTasks) { }

        public MultiThreadRunner
            (string name, float intervalInMs) : base(name, new StandardRunningInfo(), intervalInMs) { }
    }

    /// <summary>
    /// The multithread runner always uses just one thread to run all the couroutines
    /// If you want to use a separate thread, you will need to create another MultiThreadRunner 
    /// </summary>
    /// <typeparam name="TTask"></typeparam>
    /// <typeparam name="TFlowModifier"></typeparam>
    public class MultiThreadRunner<TTask, TFlowModifier> : IRunner, IRunner<TTask> where TTask : ISveltoTask
        where TFlowModifier : IFlowModifier
    {
        /// <summary>
        /// when the thread must run very tight and cache friendly tasks that won't allow the CPU to start new threads,
        /// passing the tightTasks as true would force the thread to yield every so often. Relaxed to true
        /// would let the runner be less reactive on new tasks added.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tightTasks"></param>
        public MultiThreadRunner(string name, TFlowModifier modifier, bool relaxed = false, bool tightTasks = false)
        {
            var runnerData = new RunnerData(relaxed, 0, name, tightTasks, modifier);

            Init(runnerData);
        }

        /// <summary>
        /// Start a Multithread runner that won't take 100% of the CPU
        /// </summary>
        /// <param name="name"></param>
        /// <param name="intervalInMs"></param>
        public MultiThreadRunner(string name, TFlowModifier modifier, float intervalInMs)
        {
            var runnerData = new RunnerData(true, intervalInMs, name, false, modifier);

            Init(runnerData);
        }

        ~MultiThreadRunner()
        {
            Console.LogWarning("MultiThreadRunner has been garbage collected, this could have serious"
                             + "consequences, are you sure you want this? ".FastConcat(_runnerData.name));

            Dispose();
        }

        public bool isStopping => _runnerData.waitForStop;

        public bool isKilled                => _runnerData == null;
        public bool paused                  => _runnerData.isPaused;
        public uint numberOfRunningTasks    => _runnerData.Count;
        public uint numberOfQueuedTasks     => _runnerData.newTaskRoutines.count;
        public uint numberOfProcessingTasks => _runnerData.Count + _runnerData.newTaskRoutines.count;
        public bool hasTasks                => numberOfProcessingTasks != 0;

        public override string ToString() { return _runnerData.name; }

        public void Pause()  { _runnerData.isPaused = true; }
        public void Resume() { _runnerData.isPaused = false; }

        public void Flush()
        {
            _runnerData.StopAndFlush();
        }

        public void Dispose()
        {
            if (isKilled == false)
                Kill(null);

            GC.SuppressFinalize(this);
        }

        public void StartCoroutine(in TTask task)
        {
            if (isKilled == true)
                throw new MultiThreadRunnerException("Trying to start a task on a killed runner");

            _runnerData.newTaskRoutines.Enqueue(task);
            _runnerData.UnlockThread();
        }

        public void Stop()
        {
            if (isKilled == true)
                return;

            _runnerData.Stop();
        }

        public void Kill(Action onThreadKilled)
        {
            if (isKilled == true)
                throw new MultiThreadRunnerException("Trying to kill an already killed runner");

            _runnerData.Kill(onThreadKilled);
            _runnerData = null;
        }

        public void Kill()
        {
            if (isKilled == true)
                throw new MultiThreadRunnerException("Trying to kill an already killed runner");

            _runnerData.Kill(null);
            _runnerData = null;
        }

        void Init(RunnerData runnerData)
        {
            _runnerData = runnerData;
#if !NETFX_CORE
            //threadpool doesn't work well with Unity apparently it seems to choke when too meany threads are started
            new Thread(runnerData.RunCoroutineFiber)
            {
                IsBackground = true
            }.Start();
#else
            Task.Factory.StartNew(() => runnerData.RunCoroutineFiber(), TaskCreationOptions.LongRunning);
#endif
        }

        class RunnerData
        {
            public RunnerData
                (bool relaxed, float interval, string name, bool isRunningTightTasks, TFlowModifier modifier)
            {
                _mevent              = new ManualResetEventSlim();
                _watch               = new Stopwatch();
                _coroutines          = new FasterList<TTask>();
                newTaskRoutines      = new ThreadSafeQueue<TTask>();
                _interval            = (long) (interval * 10000);
                this.name            = name;
                _isRunningTightTasks = isRunningTightTasks;
                _flushingOperation   = new SveltoTaskRunner<TTask>.FlushingOperation();
                modifier.runnerName  = name;
                _process = new SveltoTaskRunner<TTask>.Process<TFlowModifier>(
                    newTaskRoutines, _coroutines, _flushingOperation, modifier);

                if (relaxed)
                    _lockingMechanism = RelaxedLockingMechanism;
                else
                    _lockingMechanism = QuickLockingMechanism;
            }

            internal uint Count
            {
                get
                {
                    Interlocked.MemoryBarrier();

                    return (uint) _coroutines.count;
                }
            }

            internal void Stop()
            {
                _flushingOperation.Stop(name);
                //unlocking thread as otherwise the stopping flag will never be reset
                UnlockThread();
            }

            internal void StopAndFlush()
            {
                _flushingOperation.StopAndFlush();
                
                //unlocking thread as otherwise the stopping flag will never be reset
                UnlockThread();
            }

            internal void Kill(Action onThreadKilled)
            {
                _flushingOperation.Kill(name);
                
                if (_mevent != null) //already disposed
                {
                    _onThreadKilled = onThreadKilled;
                    Interlocked.MemoryBarrier();

                    UnlockThread();
                }

                if (_watch != null)
                {
                    _watch.Stop();
                    _watch = null;
                }
            }

            internal void UnlockThread()
            {
                _interlock = 1;

                _mevent.Set();

                Interlocked.MemoryBarrier();
            }

            internal void RunCoroutineFiber()
            {
                Interlocked.MemoryBarrier();

                using (var profiler = new PlatformProfilerMT(name))
                {
                    try
                    {
                        while (true)
                        {
                            if (_process.MoveNext(profiler) == false)
                                break;

                            //If the runner is not killed
                            if (_flushingOperation.stopping == false)
                            {
                                //if the runner is paused enable the locking mechanism
                                if (_flushingOperation.paused)
                                    _lockingMechanism();

                                //if there is an interval time between calls we need to wait for it
                                if (_interval > 0)
                                    WaitForInterval();

                                //if there aren't task left we put the thread in pause
                                if (_coroutines.count == 0)
                                {
                                    if (newTaskRoutines.count == 0)
                                        _lockingMechanism();
                                    else
                                        ThreadUtility.Wait(ref _yieldingCount, 16);
                                }
                                else
                                {
                                    if (_isRunningTightTasks)
                                        ThreadUtility.Wait(ref _yieldingCount, 16);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Kill(null);
                        
                        //the process must always complete naturally, otherwise the continuators won't be released.
                        while (_process.MoveNext(profiler) == true);

                        throw;
                    }
                    finally
                    {
                        _onThreadKilled?.Invoke();

                        if (_mevent != null)
                        {
                            _mevent.Dispose();
                            _mevent = null;

                            Interlocked.MemoryBarrier();
                        }
                    }
                }
            }

            internal bool isPaused
            {
                get => _flushingOperation.paused;
                set
                {
                    _flushingOperation.Pause(name);

                    if (value == false)
                        UnlockThread();
                }
            }

            internal bool waitForStop    => _flushingOperation.stopping;

            /// <summary>
            /// More reacting pause/resuming system. It spins for a while before reverting to the relaxing locking 
            /// </summary>
            void QuickLockingMechanism()
            {
                var quickIterations = 0;
                var frequency       = 1024;

                while (Volatile.Read(ref _interlock) != 1 && quickIterations < 4096)
                {
                    ThreadUtility.Wait(ref quickIterations, frequency);

                    if (waitForStop)
                        return;
                }

                if (_interlock == 0 && waitForStop == false)
                    RelaxedLockingMechanism();
                else
                    _interlock = 0;
            }

            /// <summary>
            /// Resuming a manual even can take a long time, but allow the thread to be pause and the core to be used by other threads
            /// </summary>
            void RelaxedLockingMechanism()
            {
                _mevent.Wait();

                _mevent.Reset();
            }

            void WaitForInterval()
            {
                var quickIterations = 0;
                _watch.Start();

                while (_watch.ElapsedTicks < _interval)
                {
                    if ((_interval - _watch.ElapsedTicks) < 16000)
                        ThreadUtility.Wait(ref quickIterations);
                    else
                        ThreadUtility.TakeItEasy();

                    if (waitForStop == true)
                        return;
                }

                _watch.Reset();
            }

            internal readonly ThreadSafeQueue<TTask> newTaskRoutines;
            internal readonly string                 name;

            readonly          FasterList<TTask>      _coroutines;
            readonly          long                   _interval;
            readonly          bool                   _isRunningTightTasks;
            readonly          Action                 _lockingMechanism;

            ManualResetEventSlim       _mevent;
            Action                     _onThreadKilled;
            Stopwatch                  _watch;
            int                        _interlock;
            int                        _yieldingCount;

            readonly SveltoTaskRunner<TTask>.FlushingOperation _flushingOperation;

            readonly SveltoTaskRunner<TTask>.Process<TFlowModifier> _process;
        }

        RunnerData _runnerData;
    }

    public class MultiThreadRunnerException : Exception
    {
        public MultiThreadRunnerException(string message) : base(message) { }
    }
}