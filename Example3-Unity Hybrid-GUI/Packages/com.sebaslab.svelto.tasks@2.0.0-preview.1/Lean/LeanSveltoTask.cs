#if ENABLE_PLATFORM_PROFILER || TASKS_PROFILER_ENABLED || (DEBUG && !PROFILE_SVELTO)
#define GENERATE_NAME
#endif
using System.Collections.Generic;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.Internal;

namespace Svelto.Tasks.Lean
{
    public struct LeanSveltoTask<TTask> : ISveltoTask where TTask : IEnumerator<TaskContract>
    {
        internal Continuation Run<TRunner>(TRunner runner, ref TTask task)
            where TRunner : class, IRunner<LeanSveltoTask<TTask>>
        {
            _sveltoTask = new SveltoTaskWrapper<TTask, IRunner<LeanSveltoTask<TTask>>>(task, runner);
#if GENERATE_NAME
            _hasTask = true;
#endif

#if DEBUG && !PROFILE_SVELTO
                DBC.Tasks.Check.Require(IS_TASK_STRUCT == true || task != null
                                  , "A valid enumerator is required to enable a LeanSveltTask ".FastConcat(
                                            ToString()));
#endif

#if DEBUG && !PROFILE_SVELTO
                _continuation = new Continuation(ContinuationPool.RetrieveFromPool(), runner);
#else
            _continuation = new Continuation(ContinuationPool.RetrieveFromPool());
#endif

            _threadSafeSveltoTaskStates.started = true;

            runner.StartTask(this);

            return _continuation;
        }
        
        internal void SpawnContinuingTask<TRunner>(TRunner runner, in TTask task, Continuation continuation)
            where TRunner : class, IRunner<LeanSveltoTask<TTask>>
        {
            _sveltoTask = new SveltoTaskWrapper<TTask, IRunner<LeanSveltoTask<TTask>>>(in task, runner);
#if GENERATE_NAME
            _hasTask = true;
#endif

#if DEBUG && !PROFILE_SVELTO
                DBC.Tasks.Check.Require(IS_TASK_STRUCT == true || task != null
                                  , "A valid enumerator is required to enable a LeanSveltTask ".FastConcat(
                                            ToString()));
#endif

            _continuation = continuation;

            _threadSafeSveltoTaskStates.started = true;

            runner.SpawnContinuingTask(this);
        }

        public override string ToString()
        {
#if GENERATE_NAME
            if (_name == null)
                if (_hasTask == true)
                    _name = _sveltoTask.task.ToString();
                else
                    return "LeanSveltoTask";

            return _name;
#else
            return "LeanSveltoTask";
#endif
        }

        public void Stop()
        {
            _threadSafeSveltoTaskStates.explicitlyStopped = true;
        }
        
        public bool isCompleted => _threadSafeSveltoTaskStates.completed;

        public string name => ToString();

        public bool MoveNext()
        {
            DBC.Tasks.Check.Require(_threadSafeSveltoTaskStates.completed == false, "impossible state");
            bool completed = false;

            try
            {
                if (_threadSafeSveltoTaskStates.explicitlyStopped == false)
                {
                    completed = !_sveltoTask.MoveNext();
                }
                else
                    completed = true;
            }
            finally
            {
                if (completed == true)
                {
                    _continuation.ReturnToPool();
                    _threadSafeSveltoTaskStates.completed = true;
                }
            }

            return !completed;
        }

        SveltoTaskWrapper<TTask, IRunner<LeanSveltoTask<TTask>>> _sveltoTask;
        SveltoTaskState                                          _threadSafeSveltoTaskStates;
        Continuation                                             _continuation;
#if GENERATE_NAME
        string _name;
        bool   _hasTask;
#endif
#if DEBUG && !PROFILE_SVELTO
        static readonly bool IS_TASK_STRUCT = typeof(TTask).IsValueType;
#endif
    }
}