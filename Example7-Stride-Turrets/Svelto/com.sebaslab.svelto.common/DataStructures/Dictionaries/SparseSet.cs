using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures.Experimental
{
    /// <summary>
    ///     Represents an unordered sparse set of natural numbers, and provides constant-time operations on it.
    ///     Once this class gets out of experimental, it must use BufferStrategies
    /// </summary>
    public sealed class SparseSet<T> where T : unmanaged
    {
        public SparseSet()
        {
            _sparse = new NativeStrategy<int>(1, Allocator.Persistent, false);
            _sparse[0] = -1;
            _dense  = new NativeStrategy<T>(1, Allocator.Persistent);
        }
        
        public SparseSet(uint size)
        {
            _sparse = new NativeStrategy<int>(size, Allocator.Persistent, false);
            for (int i = 0; i < size; i++)
            {
                _sparse[i] = -1;
            }
            _dense  = new NativeStrategy<T>(size, Allocator.Persistent);
        }

        public int capacity => _dense.capacity;
        public int count    => (int) _count;

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= count)
                    throw new Exception($"SparseSet - out of bound access: index {count} - capacity {count}");
#endif
                return ref _dense[index];
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= count)
                    throw new Exception($"SparseSet - out of bound access: index {count} - capacity {count}");
#endif
                return ref _dense[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _count = 0; }

        /// <summary>
        /// The common scenario of GetIndex is to be able to use an external array as a separate
        /// DenseSet too.  
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(T val)
        {
            var sparseIndex = ValToIndex(val);
            var denseIndex = _sparse[sparseIndex];
            
#if DEBUG && !PROFILE_SVELTO
            if (denseIndex == -1)
                throw new Exception("SparseSet - Item not found");
#endif
            return denseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(T val)
        {
            var index = (uint) val.GetHashCode();

            return index < capacity && _sparse[index] < count && _sparse[index] != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T val)
        {
            if (Has(val) == false)
            {
#if DEBUG && !PROFILE_SVELTO
                if (_count >= capacity)
                    throw new Exception("SparseSet is not big enough to add new elements");
#endif
                var index = ValToIndex(val);

                _dense[count]  = val;
                _sparse[index] = count;
                ++_count;

                return;
            }

#if DEBUG && !PROFILE_SVELTO
            throw new Exception("SparseSet - trying to insert already found value");
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Intersect(in SparseSet<T> otherSet)
        {
            for (uint i = 0; i < count; i++)
                if (otherSet.Has(this[i]) == false)
                    Remove(this[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T val)
        {
            if (Has(val))
            {
                var index = ValToIndex(val);
                
                _dense[_sparse[index]] = _dense[_count - 1];
                var hashCode = _dense[_count - 1].GetHashCode();
                _sparse[hashCode] = _sparse[index];
                _sparse[index]    = -1;
                --_count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(uint u)
        {
            if (u > capacity)
            {
                _dense.Resize(u);
                _sparse.Resize(u);
            }
        }

        ~SparseSet()
        {
            _sparse.Dispose();
            _dense.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint ValToIndex(T val)
        {
            var index = (uint) val.GetHashCode();

#if DEBUG && !PROFILE_SVELTO
            if (index >= capacity)
                throw new Exception("SparseSet - Unsupported GetHashCode used or out of bound value");
#endif
            return index;
        }

        readonly NativeStrategy<T>   _dense; //Dense set of elements
        readonly NativeStrategy<int> _sparse; //Map of elements to dense set indices
        int                          _count;
    }
}