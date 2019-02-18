using System;
using System.Collections;
using Svelto.Tasks.Internal;
using Svelto.Utilities;

namespace Svelto.Tasks.Enumerators
{
    /// <summary>
    /// The Continuation Wrapper contains a valid value until the task is not stopped. After that it should be released. 
    /// </summary>
    public class ContinuationEnumerator : IEnumerator
    {
        internal ContinuationEnumerator()
        {}
        
        public bool MoveNext()
        {
            if (ThreadUtility.VolatileRead(ref _completed) == true)
            {
                _completed = false;
                
                return false;
            }

            return true;
        }

        internal void ReturnToPool()
        {
            ContinuationPool.PushBack(this);
            
            ThreadUtility.VolatileWrite(ref _completed, true);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        
        ~ContinuationEnumerator()
        {
            _completed = false;

            ContinuationPool.PushBack(this);
        }

        public object Current => throw new NotImplementedException();
        
        internal void InternalReset()
        {
            _completed = false;
        }

        bool _completed;
    }
}