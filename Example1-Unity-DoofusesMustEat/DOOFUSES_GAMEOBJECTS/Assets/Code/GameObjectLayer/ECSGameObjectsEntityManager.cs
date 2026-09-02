using UnityEngine;
using Svelto.DataStructures;
using Unity.Mathematics;
using UnityEngine.Jobs;
using UnityEngine.Pool;

namespace Svelto.ECS.MiniExamples.Doofuses.GameObjects.GameobjectLayer
{
    public class ECSGameObjectsEntityManager
    {
        public ECSGameObjectsEntityManager()
        {
            _pools                = new FasterList<ObjectPool<GameObject>>();
            _transformAccessArray = new FasterList<TransformAccessArray>();
        }

        public void Dispose()
        {
            for (int i = 0; i < _pools.count; i++)
                _pools[i].Dispose();

            for (int i = 0; i < _transformAccessArray.count; i++)
                if (_transformAccessArray[i].isCreated)
                    _transformAccessArray[i].Dispose();
        }

        public uint LoadAndRegisterPrefab(string prefabName)
        {
            var prefab = Resources.Load<UnityEngine.GameObject>(prefabName);
            var prefabID = (uint)_pools.count;
            _pools.Add(CreatePool(prefab));

            return prefabID;
        }
        
        public void Swap(uint fromGroupID, uint toGroupID)
        {
            var transformsTo = _transformAccessArray.GetOrCreate(toGroupID, () => new TransformAccessArray(1));
            var transformsFrom = _transformAccessArray[fromGroupID];
            
            var elementToSwap   = transformsFrom[transformsFrom.length - 1];

            transformsTo.Add(elementToSwap);
            transformsFrom.RemoveAtSwapBack(transformsFrom.length - 1);
        }

        internal Transform InstantiatePrefab(int prefabID, uint groupID)
        {
            var go = _pools[prefabID].Get();

            var transformAccessArray = _transformAccessArray.GetOrCreate(groupID, () => new TransformAccessArray(1));
            transformAccessArray.Add(go.transform);

            return go.transform;
        }

        internal void Recycle(int prefabID, uint groupID)
        {
            var transformAccessArray = _transformAccessArray[groupID];
            var go                   = transformAccessArray[transformAccessArray.length - 1].gameObject;
            _pools[prefabID].Release(go);

            transformAccessArray.RemoveAtSwapBack(transformAccessArray.length - 1);
        }

        internal TransformAccessArray Transforms(uint groupID)
        {
            return _transformAccessArray[groupID];
        }

        static ObjectPool<GameObject> CreatePool(GameObject prefab)
        {
            return new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab)
              , actionOnGet: gameObject => gameObject.SetActive(true)
              , actionOnRelease: gameObject => gameObject.SetActive(false)
              , actionOnDestroy: gameObject => Object.Destroy(gameObject)
              , collectionCheck: true
              , maxSize: int.MaxValue);
        }

        readonly FasterList<ObjectPool<GameObject>> _pools;
        readonly FasterList<TransformAccessArray> _transformAccessArray;
    }
}
