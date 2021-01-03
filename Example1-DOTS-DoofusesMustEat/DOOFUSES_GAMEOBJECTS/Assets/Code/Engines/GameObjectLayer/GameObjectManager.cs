using System.Collections.Generic;
using Svelto.Common.DataStructures;
using Svelto.DataStructures;
using Svelto.ObjectPool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Svelto.Common.DataStructures
{
    /// <summary>
    /// Represents an unordered sparse set of natural numbers, and provides constant-time operations on it.
    /// </summary>
    public sealed class SparseSet
    {
        public readonly FasterList<int> dense; //Dense set of elements
        public readonly FasterList<int> sparse; //Map of elements to dense set indices

        int  size_     = 0; //Current size (number of elements)
        uint capacity_ = 0; //Current size (number of elements)
        
        public SparseSet()
        {
            this.sparse = new FasterList<int>(1);
            this.dense  = new FasterList<int>(1);
        }

        void clear() { size_ = 0; }

        void reserve(uint u)
        {
            if (u > capacity_)
            {
                dense.ExpandTo(u);
                sparse.ExpandTo(u);
                capacity_ = u;
            }
        }

        public bool has(int val)
        {
            return val < capacity_ && sparse[val] < size_ && dense[sparse[val]] == val;
        }

        public void insert(int val)
        {
            if (!has(val))
            {
                if (val >= capacity_)
                    reserve((uint) (val + 1));

                dense[size_] = val;
                sparse[val]  = size_;
                ++size_;
            }
        }

        public void erase(int val)
        {
            if (has(val))
            {
                dense[sparse[val]]       = dense[size_ - 1];
                sparse[dense[size_ - 1]] = sparse[val];
                --size_;
            }
        }
    };
}

namespace Svelto.ECS.MiniExamples.Example1C
{
    class GameObjectManager
    {
        public GameObjectManager()
        {
            _pool                 = new GameObjectPool();
            _prefabs              = new FasterList<GameObject>();
            _instancesMap         = new FasterDictionary<int, SparseSet>();
            _transformAccessArray = new FasterDictionary<int, TransformAccessArray>();
        }

        public TransformAccessArray Transforms(int ID) => _transformAccessArray[ID];

        public int RegisterPrefab(GameObject prefab)
        {
            _prefabs.Add(prefab);

            return _prefabs.count - 1;
        }
        
        public void SetPosition(int gameobjectID, int poolID, float3 position)
        {
            _transformAccessArray[poolID][_instancesMap[poolID].sparse[gameobjectID]].position = position;
        }

        public int FetchGameObject(int prefabID, int poolID)
        {
            GameObject ONFirstUse() => GameObject.Instantiate(_prefabs[prefabID]);

            var go = _pool.Use(poolID, ONFirstUse);
            go.SetActive(true);

            _instancesMap.GetOrCreate(poolID, () => new SparseSet()).insert(_lastIndex);
            _transformAccessArray.GetOrCreate(poolID, () => new TransformAccessArray(1)).Add(go.transform);

            return _lastIndex++;
        }

        public void Recycle(int gameObjectID, int poolID)
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
        readonly FasterDictionary<int, SparseSet> _instancesMap;
        readonly FasterDictionary<int, TransformAccessArray> _transformAccessArray;
        int                                                  _lastIndex;
    }
}