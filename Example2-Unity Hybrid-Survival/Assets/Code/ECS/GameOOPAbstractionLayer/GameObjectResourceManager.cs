using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Svelto.DataStructures;
using Svelto.ECS.ResourceManager;
using UnityEngine;
using UnityEngine.Pool;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// Holds the resources used by the game and map them to unmanaged indices usable by Svelto Components
    /// To know more about Resource Managers read: https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/
    /// </summary>
    public class GameObjectResourceManager : ECSResourceManager<GameObject>
    {
        public GameObjectResourceManager()
        {
            _resourcePools = new Dictionary<int, ObjectPool<GameObject>>();
            _factory = new GameObjectFactory();
        }

        public async Task<ValueIndex> Build(string prefabName, bool startActive = true)
        {
            var gameObject = await _factory.Build(prefabName, startActive);

            return Add(gameObject);
        }
        
        public async Task Preallocate(string prefabName, int pool, int size)
        {
            ObjectPool<GameObject> resourcePool = GetOrCreatePool(pool);

            for (int i = 0; i < size; i++)
            {
                GameObject gameObject = await _factory.Build(prefabName, false);
                resourcePool.Release(gameObject);
            }
        }

        public async Task<ValueIndex> Reuse(string prefabName, int pool)
        {
            ObjectPool<GameObject> resourcePool = GetOrCreatePool(pool);

            //objects can only be created asynchronously, so the pool is used only for actual reuse
            if (resourcePool.CountInactive > 0)
                return Add(resourcePool.Get());

            return await Build(prefabName, false); //build is async
        }

        public void Recycle(ValueIndex indextoRecycle, int pool)
        {
            GameObject gameObject = this[indextoRecycle];
            gameObject.SetActive(false);
            GetOrCreatePool(pool).Release(gameObject);
        }

        ObjectPool<GameObject> GetOrCreatePool(int pool)
        {
            if (_resourcePools.TryGetValue(pool, out ObjectPool<GameObject> resourcePool) == false)
            {
                resourcePool = new ObjectPool<GameObject>(
                    createFunc: () => throw new InvalidOperationException(
                        "pooled GameObjects must be created through the GameObjectFactory")
                  , collectionCheck: true
                  , maxSize: int.MaxValue); //keep the unbounded behaviour of the previous pool

                _resourcePools[pool] = resourcePool;
            }

            return resourcePool;
        }
        
        /// <summary>
        /// The assembly gives the opportunity to encapsulate completely objects. Only this layer can retrieve
        /// objects from the manager
        /// </summary>
        /// <param name="index"></param>
        internal new GameObject this[ValueIndex index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => base[index];
        }

        readonly GameObjectFactory _factory;
        readonly Dictionary<int, ObjectPool<GameObject>> _resourcePools;
    }
}