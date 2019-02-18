using System.Collections.Generic;

namespace Svelto.Tasks.Internal
{
    struct SveltoTaskWrapper<TTask, TRunner> where TTask : IEnumerator<TaskContract>
                                             where TRunner : class, IInternalRunner<LeanSveltoTask<TTask>> 
    {
        public SveltoTaskWrapper(ref TTask task, TRunner runner):this()
        {
            _runner = runner;
            _task   = task;
        }
        
        public SveltoTaskWrapper(ref TTask task):this()
        {
            _task   = task;
        }

        public bool MoveNext()
        {
            var continuationWrapper = Current.ContinuationEnumerator;
            if (continuationWrapper != null)
            {
                if (continuationWrapper.MoveNext() == true) 
                    return true;
                if (_continuingTask.Current.breakit == Break.AndStop) 
                    return false;
            }

            if (_task.MoveNext() == false) 
                return false;
    
            Current = _task.Current;
                 
            if (Current.yieldIt == true) 
               return true;
    
            if (Current.breakit == Break.It || Current.breakit == Break.AndStop || Current.hasValue == true)
                return false;

            if (Current.enumerator != null && _runner != null)
            {
                //Current.enumerator is a "continued" enumerator and can be only a class at the moment
                _continuingTask = (TTask) Current.enumerator;

                //a new TaskContract is created, holding the continuationEnumerator
                Current = _continuingTask.RunImmediate(_runner);
            }

            return true;
        }

        TTask             _task;
        readonly TRunner  _runner;
        TTask             _continuingTask;
        
        //must stay internal?
        internal TaskContract Current;
    }
}