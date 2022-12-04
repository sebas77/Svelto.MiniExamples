using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.ResourceManager;
using Svelto.ObjectPool;
using UnityEngine;

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
            _resourcePool = new ThreadSafeObjectPool<GameObject>();
            _factory = new GameObjectFactory();
        }

        public IEnumerable<ValueIndex?> Build(string prefabName)
        {
            var gameObject = _factory.Build(prefabName);

            while (gameObject.MoveNext()) 
                yield return null;

            yield return Add(gameObject.Current);
        }
        
        public IEnumerable<ValueIndex?> Reuse(string prefabName, int pool)
        {
            if (_resourcePool.Reuse(pool, out var obj) == false)
            {
                return Build(prefabName);
            }

            return new ValueIndex?[] {Add(obj)};
        }
        
        public void Recycle(ValueIndex indextoRecycle, int pool)
        {
            throw new NotImplementedException();
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
        readonly ThreadSafeObjectPool<GameObject> _resourcePool;
    }
}