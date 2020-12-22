using System;
using System.Collections.Generic;

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
            _namedPools.Clear();
#if DEBUG && !PROFILE_SVELTO
            alreadyRecycled.Clear();
#endif
        }

        public virtual void OnDispose()
        { }
        
        public void Dispose()
        {
            OnDispose();

            _pools.Clear();
            _namedPools.Clear();

            _disposed = true;
        }
        
        public void Clear()
        {
            _pools.Clear();
            _namedPools.Clear();
        }
        
        public virtual void Recycle(T go, int pool)
        {
            InternalRecycle(go, pool);
        }

        public virtual void Recycle(T go, string poolName)
        {
            InternalRecycle(go, poolName);
        }

        public void Preallocate(string poolName, int size, Func<T> onFirstUse)
        {
            for (int i = size - 1; i >= 0; --i)
                Create(poolName, onFirstUse);
        }

        public void Preallocate(int pool, int size, Func<T> onFirstUse)
        {
            for (int i = size - 1; i >= 0; --i)
                Create(pool, onFirstUse);
        }

        public T Use(string poolName, Func<T> onFirstUse)
        {
            Stack<T> localPool = ReturnValidPool(poolName);

            return ReuseInstance(localPool, onFirstUse);
        }

        public T Use(int pool, Func<T> onFirstUse)
        {
            Stack<T> localPool = ReturnValidPool(pool);

            return ReuseInstance(localPool, onFirstUse);
        }

        public K Use<K>(string poolName, Func<T> onFirstUse) where K : class, T
        {
            Stack<T> localPool = ReturnValidPool(poolName);

            return ReuseInstance(localPool, onFirstUse) as K;
        }

        public K Use<K>(int pool, Func<T> onFirstUse) where K : class, T
        {
            Stack<T> localPool = ReturnValidPool(pool);

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

        private void InternalRecycle(T obj, int pool)
        {
            if (_disposed) return;
#if DEBUG && !PROFILE_SVELTO
            if (alreadyRecycled.Add(obj) == false)
                throw new Exception("An object already Recycled in the pool has been Recycled again");
#endif
            DBC.Common.Check.Assert(_pools.ContainsKey(pool) == true,
                             "Cannot recycle object without being preallocated or used");

            _pools[pool].Push(obj);

            _objectsRecyled++;
        }

        private void InternalRecycle(T obj, string poolName)
        {
            if (_disposed) return;
#if DEBUG && !PROFILE_SVELTO
            if (alreadyRecycled.Add(obj) == false)
                throw new Exception("An object already Recycled in the pool has been Recycled again");
#endif
            DBC.Common.Check.Assert(_namedPools.ContainsKey(poolName) == true,
                             "Cannot recycle object without being preallocated or used");

            _namedPools[poolName].Push(obj);

            _objectsRecyled++;
        }

        protected T Create(string poolName, Func<T> onFirstInit)
        {
            Stack<T> localPool = ReturnValidPool(poolName);

            var go = onFirstInit();

            localPool.Push(go);

            return go;
        }

        protected T Create(int pool, Func<T> onFirstInit)
        {
            Stack<T> localPool = ReturnValidPool(pool);

            var go = onFirstInit();

            localPool.Push(go);

            return go;
        }

        Stack<T> ReturnValidPool(int pool)
        {
            if (_pools.TryGetValue(pool, out var localPool) == false)
                _pools[pool] = localPool = new Stack<T>();

            return localPool;
        }

        Stack<T> ReturnValidPool(string poolName)
        {
            if (_namedPools.TryGetValue(poolName, out var localPool) == false)
                localPool = _namedPools[poolName] = new Stack<T>();

            return localPool;
        }

        T ReuseInstance(Stack<T> pool, Func<T> onFirstInit)
        {
            T obj = null;

            while (IsNull(obj) == true && pool.Count > 0)
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

        static bool IsNull(object aObj)
        {
            return aObj == null || aObj.Equals(null);
        }

        protected readonly Dictionary<int, Stack<T>>    _pools      = new Dictionary<int, Stack<T>>();
        protected readonly Dictionary<string, Stack<T>> _namedPools = new Dictionary<string, Stack<T>>();

        int _objectsReused;
        int _objectsCreated;
        int _objectsRecyled;
        bool _disposed;

#if DEBUG && !PROFILE_SVELTO
        public List<ObjectPoolDebugStructureInt> DebugPoolInfo(List<ObjectPoolDebugStructureInt> debugInfo)
        {
            debugInfo.Clear();

            for (var enumerator = _pools.GetEnumerator(); enumerator.MoveNext();)
                debugInfo.Add(new ObjectPoolDebugStructureInt(enumerator.Current.Key, enumerator.Current.Value.Count));

            return debugInfo;
        }

        public List<ObjectPoolDebugStructureString> DebugNamedPoolInfo(List<ObjectPoolDebugStructureString> debugInfo)
        {
            debugInfo.Clear();

            for (var enumerator = _namedPools.GetEnumerator(); enumerator.MoveNext();)
                debugInfo.Add(new ObjectPoolDebugStructureString(enumerator.Current.Key,
                                                                 enumerator.Current.Value.Count));

            return debugInfo;
        }
#endif
    }
}