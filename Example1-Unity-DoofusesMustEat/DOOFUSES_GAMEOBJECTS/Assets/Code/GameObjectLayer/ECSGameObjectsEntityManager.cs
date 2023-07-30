using UnityEngine;
using Svelto.DataStructures;
using Svelto.ObjectPool;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Svelto.ECS.MiniExamples.Doofuses.GameObjects.GameobjectLayer
{
    public class ECSGameObjectsEntityManager
    {
        public ECSGameObjectsEntityManager()
        {
            _pool                 = new GameObjectPool();
            _prefabs              = new FasterList<GameObject>();
            _transformAccessArray = new FasterList<TransformAccessArray>();
        }

        public void Dispose()
        {
            _pool?.Dispose();
            for (int i = 0; i < _transformAccessArray.count; i++)
                if (_transformAccessArray[i].isCreated)
                    _transformAccessArray[i].Dispose();
        }

        public uint LoadAndRegisterPrefab(string prefabName)
        {
            var prefab = Resources.Load<UnityEngine.GameObject>(prefabName);
            _prefabs.Add(prefab);
            return _prefabsCount++;
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
            //optimization alert: this will allocate every time is used, not good. However it's used only 
            //when entities are created so..meh
            GameObject OnFirstUse()
            {
                return Object.Instantiate(_prefabs[prefabID]);
            }

            var go = _pool.Use(prefabID, OnFirstUse);
            go.SetActive(true);

            var transformAccessArray = _transformAccessArray.GetOrCreate(groupID, () => new TransformAccessArray(1));
            transformAccessArray.Add(go.transform);

            return go.transform;
        }

        internal void Recycle(int prefabID, uint groupID)
        {
            var transformAccessArray = _transformAccessArray[groupID];
            var go                   = transformAccessArray[transformAccessArray.length - 1].gameObject;
            _pool.Recycle(go, prefabID);
            go.SetActive(false);

            transformAccessArray.RemoveAtSwapBack(transformAccessArray.length - 1);
        }

        internal TransformAccessArray Transforms(uint groupID)
        {
            return _transformAccessArray[groupID];
        }

        readonly GameObjectPool                   _pool;
        readonly FasterList<GameObject>           _prefabs;
        readonly FasterList<TransformAccessArray> _transformAccessArray;

        uint        _prefabsCount;
    }
}