using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public async Task<ValueIndex> Build(string prefabName, bool startActive = true)
        {
            var gameObject = await _factory.Build(prefabName, startActive);

            return Add(gameObject);
        }
        
        public async Task Preallocate(string prefabName, int pool, int size)
        {
            await _resourcePool.Preallocate(pool, size, () => _factory.Build(prefabName, false)); 
        }
        
        public async Task<ValueIndex> Reuse(string prefabName, int pool)
        {
            if (_resourcePool.TryReuse(pool, out var obj) == false)
            {
                return await Build(prefabName, false); //build is async
            }

            return Add(obj);
        }
        
        public void Recycle(ValueIndex indextoRecycle, int pool)
        {
            GameObject gameObject = this[indextoRecycle];
            gameObject.SetActive(false);
            _resourcePool.Recycle(gameObject, pool);
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