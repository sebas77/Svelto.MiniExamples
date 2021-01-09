using Svelto.Common.DataStructures;
using Svelto.DataStructures;
using Svelto.ObjectPool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    public class GameObjectManager
    {
        public GameObjectManager()
        {
            _pool                 = new GameObjectPool();
            _prefabs              = new FasterList<GameObject>();
            _instancesMap         = new FasterDictionary<int, SparseSet>();
            _transformAccessArray = new FasterDictionary<int, TransformAccessArray>();
        }

        internal TransformAccessArray Transforms(int ID) => _transformAccessArray[ID];

        public int RegisterPrefab(GameObject prefab)
        {
            _prefabs.Add(prefab);

            return _prefabs.count - 1;
        }

        internal void SetPosition(int gameobjectID, int poolID, float3 position)
        {
            _transformAccessArray[poolID][_instancesMap[poolID].sparse[gameobjectID]].position = position;
        }

        internal int FetchGameObject(int prefabID, int poolID)
        {
            //optimization alert: this will allocate every time is used, not good. However it's used only 
            //when entities are created so..meh
            GameObject ONFirstUse() => GameObject.Instantiate(_prefabs[prefabID]);

            var go = _pool.Use(poolID, ONFirstUse);
            go.SetActive(true);

            _instancesMap.GetOrCreate(poolID, () => new SparseSet()).insert(_lastIndex);
            _transformAccessArray.GetOrCreate(poolID, () => new TransformAccessArray(1)).Add(go.transform);

            return _lastIndex++;
        }

        internal void Recycle(int gameObjectID, int poolID)
        {
            var sparseSet            = _instancesMap[poolID];
            var index                = sparseSet.sparse[gameObjectID];
            var transformAccessArray = _transformAccessArray[poolID];
            var go                   = transformAccessArray[index].gameObject;
            _pool.Recycle(go, (int) poolID);
            go.SetActive(false);

            sparseSet.erase(gameObjectID);
            transformAccessArray.RemoveAtSwapBack(index);
        }

        readonly GameObjectPool                              _pool;
        readonly FasterList<GameObject>                      _prefabs;
        readonly FasterDictionary<int, SparseSet>            _instancesMap;
        readonly FasterDictionary<int, TransformAccessArray> _transformAccessArray;
        int                                                  _lastIndex;
    }
}