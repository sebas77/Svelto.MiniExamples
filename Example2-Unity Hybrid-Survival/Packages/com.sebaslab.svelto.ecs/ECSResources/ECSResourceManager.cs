using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Code.ECS.Shared
{
    /// <summary>
    ///     Inherit this class to have the base functionalities to implement a custom ECS compatible resource manager
    /// </summary>
    public class ECSResourceManager<T>
    {
        readonly uint[]        _sparse; //maybe one day this could become a bit array if it would ever make sense
        readonly FasterList<T> _dense;

        protected ECSResourceManager(uint maxSparseSize)
        {
            _sparse = new uint[maxSparseSize];
            _dense  = new FasterList<T>();
        }

        protected uint Add(T gameObjectCurrent)
        {
            var denseCount = (uint)_dense.count;
            _sparse[denseCount] = denseCount;

            _dense.Add(gameObjectCurrent);

            return denseCount;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _dense.SmartClear();
        }

        protected void Add(in T resource)
        {
            _sparse[_dense.count] = (uint)_dense.count;

            _dense.Add(resource);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint val)
        {
            var index = (uint)val.GetHashCode();
        
            return index < _sparse.Length && _sparse[index] < _dense.count && _sparse[index] != -1;
        }

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= _sparse.Length)
                    throw new Exception($"SparseSet - out of bound access: index {index} - capacity {_sparse.Length}");
#endif
                return _dense[index];
            }
        }
    }
}