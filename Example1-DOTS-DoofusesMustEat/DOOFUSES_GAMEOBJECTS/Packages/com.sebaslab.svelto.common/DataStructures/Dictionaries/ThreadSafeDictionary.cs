using System;

namespace Svelto.DataStructures
{
    /// <summary>
    ///     original code: http://devplanet.com/blogs/brianr/archive/2008/09/29/thread-safe-dictionary-update.aspx
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ThreadSafeDictionary<TKey, TValue> : ISveltoDictionary<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
    {
        public ThreadSafeDictionary(int size)
        {
            _dict = new FasterDictionary<TKey, TValue>((uint) size);
        }

        public ThreadSafeDictionary()
        {
            _dict = new FasterDictionary<TKey, TValue>();
        }

        public void Dispose()
        {
            _dict.Dispose();
        }

        public int count
        {
            get
            {
                _lockQ.EnterReadLock();
                try
                {
                    return _dict.count;                    
                }
                finally
                {
                    _lockQ.QuittingReadLock();
                }
            }
        }

        public void Add(TKey key, in TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Add(key, in value);
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public void Set(TKey key, in TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Set(key, in value);
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public void Clear()
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Clear();
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public void FastClear()
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.FastClear();
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            _lockQ.EnterReadLock();
            try
            {
                return _dict.ContainsKey(key);                
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue result)
        {
            _lockQ.EnterReadLock();
            try
            {
                return _dict.TryGetValue(key, out result);
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        public ref TValue GetOrCreate(TKey key)
        {
            _lockQ.EnterWriteLock();
            try
            {
                return ref _dict.GetOrCreate(key);
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public ref TValue GetOrCreate(TKey key, Func<TValue> builder)
        {
            _lockQ.EnterWriteLock();
            try
            {
                return ref _dict.GetOrCreate(key, builder);
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public ref TValue GetDirectValueByRef(uint index)
        {
            _lockQ.EnterReadLock();
            try
            {
                return ref _dict.GetDirectValueByRef(index);
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        public ref TValue GetValueByRef(TKey key)
        {
            _lockQ.EnterReadLock();
            try
            {
                return ref _dict.GetValueByRef(key);
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        public void ResizeTo(uint size)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.ResizeTo(size);
            }
            finally
            {
                _lockQ.EnterWriteLock();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                _lockQ.EnterReadLock();
                try
                {
                    return _dict[key];
                }
                finally
                {
                    _lockQ.QuittingReadLock();
                }
            }
            set
            {
                _lockQ.EnterWriteLock();
                try
                {
                    _dict[key] = value;
                }
                finally
                {
                    _lockQ.QuittingWriteLock();
                }
            }
        }

        public bool Remove(TKey key)
        {
            _lockQ.EnterWriteLock();
            try
            {
                return _dict.Remove(key);
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public void Trim()
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Trim();
            }
            finally
            {
                _lockQ.QuittingWriteLock();
            }
        }

        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            _lockQ.EnterReadLock();
            try
            {
                return _dict.TryFindIndex(key, out findIndex);
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        public uint GetIndex(TKey key)
        {
            _lockQ.EnterReadLock();
            try
            {
                return _dict.GetIndex(key);
            }
            finally
            {
                _lockQ.QuittingReadLock();
            }
        }

        readonly FasterDictionary<TKey, TValue> _dict;
        readonly ReaderWriterLockSlimEx         _lockQ = ReaderWriterLockSlimEx.Create();
    }
}