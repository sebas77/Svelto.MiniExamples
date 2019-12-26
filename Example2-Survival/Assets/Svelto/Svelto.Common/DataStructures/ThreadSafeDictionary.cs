using System;
using System.Collections.Generic;
using System.Threading;

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
            dict = new FasterDictionary<TKey, TValue>((uint) size);
        }

        public ThreadSafeDictionary()
        {
            dict = new FasterDictionary<TKey, TValue>();
        }

        // setup the lock;
        public uint Count
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return (uint)dict.Count;
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return dict[key];
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }

            set
            {
                LockQ.EnterWriteLock();
                try
                {
                    dict[key] = value;
                }
                finally
                {
                    LockQ.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Clear();
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public void Add(TKey key, TValue value)
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Add(key, value);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }
        
        public void Add(TKey key, ref TValue value)
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Add(key, value);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            LockQ.EnterReadLock();
            try
            {
                return dict.ContainsKey(key);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            LockQ.EnterWriteLock();
            try
            {
                return dict.Remove(key);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            LockQ.EnterReadLock();
            try
            {
                return dict.TryGetValue(key, out value);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        /// <summary>
        ///   Merge does a blind remove, and then add.  Basically a blind Upsert.
        /// </summary>
        /// <param name = "key">Key to lookup</param>
        /// <param name = "newValue">New Value</param>
        public void MergeSafe(TKey key, TValue newValue)
        {
            LockQ.EnterWriteLock();
            try
            {
                // take a writelock immediately since we will always be writing
                if (dict.ContainsKey(key))
                    dict.Remove(key);

                dict.Add(key, newValue);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        /// <summary>
        ///   This is a blind remove. Prevents the need to check for existence first.
        /// </summary>
        /// <param name = "key">Key to remove</param>
        public void RemoveSafe(TKey key)
        {
            LockQ.EnterReadLock();
            try
            {
                if (dict.ContainsKey(key))
                    LockQ.EnterWriteLock();
                try
                {
                    dict.Remove(key);
                }
                finally
                {
                    LockQ.ExitWriteLock();
                }
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        // This is the internal dictionary that we are wrapping
        readonly FasterDictionary<TKey, TValue> dict;

        readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim();

        public void Update(TKey key, ref TValue value)
        {
            LockQ.EnterWriteLock();
            try
            {
                dict[key] = value;
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }
    }
}
