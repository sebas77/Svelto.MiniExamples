using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Code.ECS.Shared
{
    /// <summary>
    ///     Inherit this class to have the base functionalities to implement a custom ECS compatible resource manager
    /// </summary>
    public class ECSResourceManager<T> where T:class
    {
        readonly FasterList<uint>        _sparse; //maybe one day this could become a bit array if it would ever make sense
        readonly FasterList<T> _dense;

        static readonly EqualityComparer<T> _comparer = EqualityComparer<T>.Default;

        protected ECSResourceManager(uint maxSparseSize)
        {
            _sparse = new FasterList<uint>(maxSparseSize);
            _dense  = new FasterList<T>();
        }

        protected uint Add(in T resource)
        {
            var denseCount = (uint)_dense.count;
            _sparse[denseCount] = denseCount;

            _dense.Add(resource);

            return denseCount;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _sparse.FastClear();
            _dense.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint id, in T value)
        {
            return id < _sparse.count && _sparse[id] < _dense.count && _comparer.Equals(_dense[_sparse[id]], value);
        }

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= _sparse.count)
                    throw new Exception($"SparseSet - out of bound access: index {index} - capacity {_sparse.count}");
#endif
                return _dense[index];
            }
        }
    }
}