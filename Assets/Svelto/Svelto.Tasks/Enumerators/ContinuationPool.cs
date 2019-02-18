using System;
using Svelto.DataStructures;
using Svelto.Tasks.Enumerators;

namespace Svelto.Tasks.Internal
{
    static class ContinuationPool
    {
        static ContinuationPool()
        {
            for (int i = 0; i < 1000; i++) _pool.Enqueue(new ContinuationEnumerator());
        }
        
        public static ContinuationEnumerator RetrieveFromPool()
        {
            ContinuationEnumerator task;

            if (_pool.Dequeue(out task))
            {
                GC.ReRegisterForFinalize(task);

                return task;
            }

            return CreateEmpty();
        }

        public static void PushBack(ContinuationEnumerator task)
        {
            GC.SuppressFinalize(task); //will be register again once pulled from the pool
            _pool.Enqueue(task);
        }

        static ContinuationEnumerator CreateEmpty() 
        {
            return new ContinuationEnumerator();
        }

        static readonly LockFreeQueue<ContinuationEnumerator> _pool = new LockFreeQueue<ContinuationEnumerator>();
    }
}
