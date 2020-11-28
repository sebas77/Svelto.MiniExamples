using System;

namespace Svelto.DataStructures
{
    /// <summary>
    ///   original code: http://devplanet.com/blogs/brianr/archive/2008/09/29/thread-safe-dictionary-update.aspx
    ///   simplified (not an IDictionary) and apdated (uses NewFasterList)
    /// </summary>
    /// <typeparam name = "TKey"></typeparam>
    /// <typeparam name = "TValue"></typeparam>

    public sealed class ThreadSafeDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        public ThreadSafeDictionary(int size)
        {
            _dict = new FasterDictionary<TKey, TValue>((uint) size);
        }

        public ThreadSafeDictionary()
        {
            _dict = new FasterDictionary<TKey, TValue>();
        }

        // setup the lock;
        public uint Count
        {
            get
            {
                _lockQ.EnterReadLock();
                try
                {
                    return (uint)_dict.count;
                }
                finally
                {
                    _lockQ.ExitReadLock();
                }
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
                    _lockQ.ExitReadLock();
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
                    _lockQ.ExitWriteLock();
                }
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
                _lockQ.ExitWriteLock();
            }
        }

        public void Add(TKey key, TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Add(key, value);
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }
        
        public void Add(TKey key, ref TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Add(key, value);
            }
            finally
            {
                _lockQ.ExitWriteLock();
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
                _lockQ.ExitReadLock();
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
                _lockQ.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            _lockQ.EnterReadLock();
            try
            {
                return _dict.TryGetValue(key, out value);
            }
            finally
            {
                _lockQ.ExitReadLock();
            }
        }

        /// <summary>
        ///   Merge does a blind remove, and then add.  Basically a blind Upsert.
        /// </summary>
        /// <param name = "key">Key to lookup</param>
        /// <param name = "newValue">New Value</param>
        public void MergeSafe(TKey key, TValue newValue)
        {
            _lockQ.EnterWriteLock();
            try
            {
                // take a writelock immediately since we will always be writing
                if (_dict.ContainsKey(key))
                    _dict.Remove(key);

                _dict.Add(key, newValue);
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        /// <summary>
        ///   This is a blind remove. Prevents the need to check for existence first.
        /// </summary>
        /// <param name = "key">Key to remove</param>
        public void RemoveSafe(TKey key)
        {
            _lockQ.EnterReadLock();
            try
            {
                if (_dict.ContainsKey(key))
                    _lockQ.EnterWriteLock();
                try
                {
                    _dict.Remove(key);
                }
                finally
                {
                    _lockQ.ExitWriteLock();
                }
            }
            finally
            {
                _lockQ.ExitReadLock();
            }
        }

        public void Update(TKey key, ref TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict[key] = value;
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }

        public void Set(TKey key, TValue value)
        {
            _lockQ.EnterWriteLock();
            try
            {
                _dict.Set(key, value);
            }
            finally
            {
                _lockQ.ExitWriteLock();
            }
        }
        
        public void CopyValuesTo(TValue[] tasks, uint index)
        {
            _lockQ.EnterReadLock();
            try
            {
                _dict.CopyValuesTo(tasks, index);
            }
            finally
            {
                _lockQ.ExitReadLock();
            }
        }

        public void CopyValuesTo(FasterList<TValue> values)
        {
            values.ExpandTo(_dict.count);
            CopyValuesTo(values.ToArrayFast(out _), 0);
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
                _lockQ.ExitWriteLock();
            }
        }

        readonly FasterDictionary<TKey, TValue> _dict;
        readonly ReaderWriterLockSlimEx _lockQ = ReaderWriterLockSlimEx.Create();
    }
}
