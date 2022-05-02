using System;
using System.Collections.Concurrent;
using Svelto.Common.DataStructures;
using Svelto.DataStructures;
#if DEBUG && !PROFILE_SVELTO
using System.Collections.Generic;
#endif


namespace Svelto.ObjectPool
{
    public class ThreadSafeObjectPool<T> : IObjectPool<T>, IDisposable
#if DEBUG && !PROFILE_SVELTO
                       , IObjectPoolDebug
#endif
        where T : class
    {
        public ThreadSafeObjectPool()
        {
            _recycledPools.Clear();
#if DEBUG && !PROFILE_SVELTO
            _alreadyRecycled.Clear();
#endif
        }

        public ThreadSafeObjectPool(Func<T> onFirstUse) : base()
        {
            _onFirstUse = onFirstUse;
        }

        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            OnDispose();

            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                using (var recycledPoolsGetValues = _recycledPools.GetValues)
                {
                    var values = recycledPoolsGetValues.GetValues(out var count);
                    for (int i = 0; i < count; i++)                     
                    {
                        using (var stacks = values[i].GetValues)
                        {
                            var stackValues = stacks.GetValues();
                            foreach (var obj in stackValues)
                                ((IDisposable)obj).Dispose();
                        }
                    }
                }

            _recycledPools.Clear();

            _disposed = true;
        }

        public void Clear()
        {
            _recycledPools.Clear();
        }

        public virtual void Recycle(T go, int pool)
        {
            InternalRecycle(go, pool);
        }

        public virtual void Recycle(T go)
        {
            InternalRecycle(go, 0);
        }

        public void Preallocate(int pool, int size, Func<T> onFirstUse)
        {
            for (int i = size - 1; i >= 0; --i)
                Create(pool, onFirstUse);
        }

        public T Use(int pool, Func<T> onFirstUse)
        {
            return ReuseInstance(pool, onFirstUse);
        }

        public T Use(Func<T> onFirstUse)
        {
            return ReuseInstance(0, onFirstUse);
        }

        public T Use(int pool)
        {
            DBC.Common.Check.Require(_onFirstUse != null, "You need to pass a function to create the object");

            return ReuseInstance(pool, _onFirstUse);
        }

        public T Use()
        {
            DBC.Common.Check.Require(_onFirstUse != null, "You need to pass a function to create the object");

            return ReuseInstance(0, _onFirstUse);
        }

        public int GetNumberOfObjectsCreatedSinceLastTime()
        {
            var ret = _objectsCreated;

            _objectsCreated = 0;

            return ret;
        }

        public int GetNumberOfObjectsReusedSinceLastTime()
        {
            var ret = _objectsReused;

            _objectsReused = 0;

            return ret;
        }

        public int GetNumberOfObjectsRecycledSinceLastTime()
        {
            var ret = _objectsRecyled;

            _objectsRecyled = 0;

            return ret;
        }

        // public void RecycleAll(int pool)
        // {
        //     if (_usedPools.TryGetValue(pool, out var list))
        //     {
        //         foreach (var obj in list)
        //             Recycle(obj, pool);
        //     }
        //
        //     _usedPools.Clear();
        // }

        // public void RecycleAll()
        // {
        //     RecycleAll(0);
        // }

#if DEBUG && !PROFILE_SVELTO
        public List<ObjectPoolDebugStructure> DebugPoolInfo(List<ObjectPoolDebugStructure> debugInfo)
        {
            debugInfo.Clear();

            //todo put it back
            // for (var enumerator = _recycledPools.GetEnumerator(); enumerator.MoveNext();)
            // {
            //     var currentValue = enumerator.Current.Value;
            //     debugInfo.Add(new ObjectPoolDebugStructure(enumerator.Current.Key, currentValue.Count));
            // }

            return debugInfo;
        }
#endif

        protected T Create(int pool, Func<T> onFirstInit)
        {
            var localPool = ReturnValidPool(_recycledPools, pool);

            var go = onFirstInit();

            localPool.Push(go);

            return go;
        }

        void InternalRecycle(T obj, int pool)
        {
            if (_disposed)
                return;
#if DEBUG && !PROFILE_SVELTO
            if (_alreadyRecycled.TryAdd(obj, true) == false)
                throw new Exception("An object already Recycled in the pool has been Recycled again");
#endif
            DBC.Common.Check.Assert(_recycledPools.ContainsKey(pool) == true, "invalid pool requested");

            _recycledPools[pool].Push(obj);

            _objectsRecyled++;
        }

        ThreadSafeStack<T> ReturnValidPool(ThreadSafeDictionary<int, ThreadSafeStack<T>> pools, int pool)
        {
            if (pools.TryGetValue(pool, out var localPool) == false)
                pools[pool] = localPool = new ThreadSafeStack<T>();

            return localPool;
        }

        T ReuseInstance(int pool, Func<T> onFirstInit)
        {
            T obj = null;

            ThreadSafeStack<T> localPool = ReturnValidPool(_recycledPools, pool);

            while (IsNull(obj) == true && localPool.count > 0)
                localPool.TryPop(out obj);

            if (IsNull(obj) == true)
            {
                obj = onFirstInit();

                _objectsCreated++;
            }
            else
            {
#if DEBUG && !PROFILE_SVELTO
                _alreadyRecycled.TryRemove(obj, out _);
#endif
                _objectsReused++;
            }

            // ConcurrentStack<T> localUsedPool = ReturnValidPool(_usedPools, pool);
            // localUsedPool.Push(obj);

            return obj;
        }

        static bool IsNull(object aObj)
        {
            return aObj == null || aObj.Equals(null);
        }

        protected readonly ThreadSafeDictionary<int, ThreadSafeStack<T>> _recycledPools =
            new ThreadSafeDictionary<int, ThreadSafeStack<T>>();

        // readonly ConcurrentDictionary<int, ConcurrentStack<T>> _usedPools =
        //     new ConcurrentDictionary<int, ConcurrentStack<T>>();
        
        readonly Func<T> _onFirstUse;
#if DEBUG && !PROFILE_SVELTO
        readonly ConcurrentDictionary<T, bool> _alreadyRecycled = new ConcurrentDictionary<T, bool>();
#endif

        int _objectsReused;
        int _objectsCreated;
        int _objectsRecyled;

        bool _disposed;
    }
}