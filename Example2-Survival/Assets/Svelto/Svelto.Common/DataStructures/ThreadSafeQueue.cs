using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public class ThreadSafeQueue<T>
    {
        public ThreadSafeQueue()
        {
            _queue = new FasterList<T>(1);
        }

        public ThreadSafeQueue(int capacity)
        {
            _queue = new FasterList<T>(capacity);
        }

        public void Enqueue(T item)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _queue.Enqueue(item);
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public ref readonly T Dequeue()
        {
            _lockQ.EnterWriteLock();
            try
            {
                return ref _queue.Dequeue();
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public void EnqueueAll(IEnumerable<T> ItemsToQueue)
        {
            _lockQ.EnterWriteLock();
            try
            {
                foreach (T item in ItemsToQueue)
                    _queue.Enqueue(item);
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public FasterList<T> DequeueAll()
        {
            FasterList<T> returnList = new FasterList<T>(_queue.Count);
            
            _lockQ.EnterWriteLock();
            try
            {
                while (_queue.Count > 0)
                    returnList.Add(_queue.Dequeue());

                return returnList;
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public void DequeueAllInto(FasterList<T> list)
        {
            int i = list.Count;
                
            list.ExpandBy((uint) _queue.Count);
            
            var array = list.ToArrayFast();
            
            _lockQ.EnterWriteLock();
            try
            {
                while (_queue.Count > 0)
                    array[i++] = _queue.Dequeue();
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public void DequeueInto(FasterList<T> list, int count)
        {
            _lockQ.EnterWriteLock();
            try
            {
                int originalSize = _queue.Count;
                while (_queue.Count > 0 && originalSize - _queue.Count < count)
                    list.Add(_queue.Dequeue());
            }   
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public FasterList<U> DequeueAllAs<U>() where U:class
        {
            FasterList<U> returnList = new FasterList<U>();
            _lockQ.EnterWriteLock();
            try
            {
                while (_queue.Count > 0)
                    returnList.Add(_queue.Dequeue() as U);

                return returnList;
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public T Peek()
        {
            T item = default(T);
            
            _lockQ.EnterReadLock();
            try
            {
                if (_queue.Count > 0)
                    item = _queue.Peek();
            }
            finally
            {
                _lockQ.ExitReadLock();
            }
            
            return item;
        }

        public void Clear()
        {
            _lockQ.EnterWriteLock();
            try
            {
                _queue.Clear();
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public bool TryDequeue(out T item)
        {
            _lockQ.EnterUpgradableReadLock();
            try
            {
                if (_queue.Count > 0)
                {
                    _lockQ.EnterWriteLock();
                    try
                    {
                        item = _queue.Dequeue();
                    }
                    finally
                    {
                        _lockQ.ExitWriteLock();
                    }
                    return true;
                }

                item = default(T);
                
                return false;
            }
            finally
            {
                _lockQ.ExitUpgradableReadLock();
            }
        }

        public int Count
        {
            get
            {
                _lockQ.EnterReadLock();
                try
                {
                    return _queue.Count;
                }
                finally
                {
                    _lockQ.ExitReadLock();
                }
            }
        }
        
        readonly FasterList<T>             _queue;
        readonly ReaderWriterLockSlimEx _lockQ = ReaderWriterLockSlimEx.Create();
    }
}
