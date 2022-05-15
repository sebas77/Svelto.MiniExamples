using System;
using Svelto.DataStructures;
using Svelto.ObjectPool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Object = UnityEngine.Object;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    public class GameObjectManager : IDisposable
    {
        public GameObjectManager()
        {
            _pool                 = new GameObjectPool();
            _prefabs              = new FasterList<GameObject>();
            _instancesMap         = new FasterDictionary<int, FasterDictionary<int, int>>();
            _transformAccessArray = new FasterDictionary<int, TransformAccessArray>();
        }

        public void Dispose()
        {
            _pool?.Dispose();
            _instancesMap?.Dispose();
            for (int i = 0; i < _transformAccessArray.count; i++)
                _transformAccessArray.unsafeValues[i].Dispose();
                    
        }

        public int RegisterPrefab(GameObject prefab)
        {
            _prefabs.Add(prefab);

            return _prefabs.count - 1;
        }

        internal int FetchGameObject(int prefabID, int poolID)
        {
            //optimization alert: this will allocate every time is used, not good. However it's used only 
            //when entities are created so..meh
            GameObject OnFirstUse()
            {
                return Object.Instantiate(_prefabs[prefabID]);
            }

            var go = _pool.Use(poolID, OnFirstUse);
            go.SetActive(true);

            _instancesMap.GetOrAdd(poolID, () => new FasterDictionary<int, int>()).Add(_lastIndex, _lastIndex);
            _transformAccessArray.GetOrAdd(poolID, () => new TransformAccessArray(1)).Add(go.transform);

            return _lastIndex++;
        }

        internal void Recycle(int gameObjectID, int poolID)
        {
            var sparseSet            = _instancesMap[poolID];
            var index                = sparseSet.GetIndex(gameObjectID);
            var transformAccessArray = _transformAccessArray[poolID];
            var go                   = transformAccessArray[(int) index].gameObject;
            _pool.Recycle(go, poolID);
            go.SetActive(false);

            sparseSet.Remove(gameObjectID);
            transformAccessArray.RemoveAtSwapBack((int) index);
        }

        internal void SetPosition(int gameobjectID, int poolID, float3 position)
        {
            _transformAccessArray[poolID][(int) _instancesMap[poolID].GetIndex(gameobjectID)].position = position;
        }

        internal TransformAccessArray Transforms(int ID)
        {
            return _transformAccessArray[ID];
        }

        readonly FasterDictionary<int, FasterDictionary<int, int>> _instancesMap;

        readonly GameObjectPool                              _pool;
        readonly FasterList<GameObject>                      _prefabs;
        readonly FasterDictionary<int, TransformAccessArray> _transformAccessArray;
        int                                                  _lastIndex;
    }
}