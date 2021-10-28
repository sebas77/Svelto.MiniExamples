using System;
using Svelto.DataStructures;
#if DEBUG && !PROFILE_SVELTO
using System.Collections.Generic;
#endif


namespace Svelto.ObjectPool
{
    public class ObjectPool<T> : IObjectPool<T>, IDisposable
#if DEBUG && !PROFILE_SVELTO
                               , IObjectPoolDebug
#endif
        where T : class
    {
#if DEBUG && !PROFILE_SVELTO
        readonly HashSet<T> alreadyRecycled = new HashSet<T>();
#endif

        public ObjectPool()
        {
            _pools.Clear();
#if DEBUG && !PROFILE_SVELTO
            alreadyRecycled.Clear();
#endif
        }

        public virtual void OnDispose() { }

        public void Dispose()
        {
            OnDispose();

            _pools.Clear();

            _disposed = true;
        }

        public void Clear() { _pools.Clear(); }

        public virtual void Recycle(T go, int pool) { InternalRecycle(go, pool); }

        public void Preallocate(int pool, int size, Func<T> onFirstUse)
        {
            for (int i = size - 1; i >= 0; --i)
                Create(pool, onFirstUse);
        }

        public T Use(int pool, Func<T> onFirstUse)
        {
            FasterList<T> localPool = ReturnValidPool(pool);

            return ReuseInstance(localPool, onFirstUse);
        }

        public K Use<K>(int pool, Func<T> onFirstUse) where K : class, T
        {
            FasterList<T> localPool = ReturnValidPool(pool);

            return ReuseInstance(localPool, onFirstUse) as K;
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

#if DEBUG && !PROFILE_SVELTO
        public List<ObjectPoolDebugStructure> DebugPoolInfo(List<ObjectPoolDebugStructure> debugInfo)
        {
            debugInfo.Clear();

            for (var enumerator = _pools.GetEnumerator(); enumerator.MoveNext();)
            {
                FasterList<T> currentValue = enumerator.Current.Value;
                debugInfo.Add(new ObjectPoolDebugStructure(enumerator.Current.Key, currentValue.count));
            }

            return debugInfo;
        }
#endif

        protected T Create(int pool, Func<T> onFirstInit)
        {
            var localPool = ReturnValidPool(pool);

            var go = onFirstInit();

            localPool.Push(go);

            return go;
        }

        void InternalRecycle(T obj, int pool)
        {
            if (_disposed)
                return;
#if DEBUG && !PROFILE_SVELTO
            if (alreadyRecycled.Add(obj) == false)
                throw new Exception("An object already Recycled in the pool has been Recycled again");
#endif
            DBC.Common.Check.Assert(_pools.ContainsKey(pool) == true, "invalid pool requested");

            _pools[pool].Push(obj);

            _objectsRecyled++;
        }

        FasterList<T> ReturnValidPool(int pool)
        {
            if (_pools.TryGetValue(pool, out var localPool) == false)
                _pools[pool] = localPool = new FasterList<T>();

            return localPool;
        }

        T ReuseInstance(FasterList<T> pool, Func<T> onFirstInit)
        {
            T obj = null;

            while (IsNull(obj) == true && pool.count > 0)
                obj = pool.Pop();

            if (IsNull(obj) == true)
            {
                obj = onFirstInit();

                _objectsCreated++;
            }
            else
            {
#if DEBUG && !PROFILE_SVELTO
                alreadyRecycled.Remove(obj);
#endif
                _objectsReused++;
            }

            return obj;
        }

        static bool IsNull(object aObj) { return aObj == null || aObj.Equals(null); }

        protected readonly FasterDictionary<int, FasterList<T>> _pools = new FasterDictionary<int, FasterList<T>>();

        int _objectsReused;
        int _objectsCreated;
        int _objectsRecyled;

        bool _disposed;
    }
}