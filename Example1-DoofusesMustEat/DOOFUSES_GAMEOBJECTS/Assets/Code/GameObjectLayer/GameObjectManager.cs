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
            _transformAccessArray = new FasterDictionary<int, TransformAccessArray>();
        }

        public void Dispose()
        {
            _pool?.Dispose();
            for (int i = 0; i < _transformAccessArray.count; i++)
                _transformAccessArray.unsafeValues[i].Dispose();
                    
        }

        public int RegisterPrefab(GameObject prefab)
        {
            _prefabs.Add(prefab);

            return _prefabs.count - 1;
        }

        internal int FetchGameObject(int poolID, int prefabID)
        {
            //optimization alert: this will allocate every time is used, not good. However it's used only 
            //when entities are created so..meh
            GameObject OnFirstUse()
            {
                return Object.Instantiate(_prefabs[prefabID]);
            }

            var go = _pool.Use(poolID, OnFirstUse);
            go.SetActive(true);

            var transformAccessArray = _transformAccessArray.GetOrAdd(poolID, () => new TransformAccessArray(1));
            transformAccessArray.Add(go.transform);

            return transformAccessArray.length - 1;
        }

        internal void Recycle(int poolID)
        {
            var transformAccessArray = _transformAccessArray[poolID];
            var go                   = transformAccessArray[transformAccessArray.length - 1].gameObject;
            _pool.Recycle(go, poolID);
            go.SetActive(false);

            transformAccessArray.RemoveAtSwapBack((int) transformAccessArray.length - 1);
        }

        internal void SetPosition(int gameobjectID, int poolID, float3 position)
        {
            _transformAccessArray[poolID][gameobjectID].position = position;
        }

        internal TransformAccessArray Transforms(int ID)
        {
            return _transformAccessArray[ID];
        }

        readonly GameObjectPool                              _pool;
        readonly FasterList<GameObject>                      _prefabs;
        readonly FasterDictionary<int, TransformAccessArray> _transformAccessArray;
        
        int                                                  _lastIndex;
    }
}