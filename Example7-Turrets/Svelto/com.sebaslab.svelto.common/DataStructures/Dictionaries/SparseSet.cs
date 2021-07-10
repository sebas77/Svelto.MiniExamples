using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// Represents an unordered sparse set of natural numbers, and provides constant-time operations on it.
    /// </summary>
    public sealed class SparseSet<T> where T : unmanaged, IConvertToUInt
    {
        readonly FasterList<T>   dense; //Dense set of elements
        readonly FasterList<int> sparse; //Map of elements to dense set indices

        public SparseSet()
        {
            this.sparse = new FasterList<int>(1);
            this.dense  = new FasterList<T>(1);
        }

        void clear() { _size = 0; }

        void reserve(uint u)
        {
            if (u > _capacity)
            {
                dense.ExpandTo(u);
                sparse.ExpandTo(u);
                _capacity = u;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool has(T tval)
        {
            unsafe
            {
                uint val = tval.ToUint();
                return val < _capacity && sparse[val] < _size && dense[sparse[val]].ToUint() == val;
            }
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (index >= _capacity)
                        throw new Exception(
                            $"NativeBuffer - out of bound access: index {index} - capacity {_capacity}");
#endif
                    return ref dense[index];
                }
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (index >= _capacity)
                        throw new Exception(
                            $"NativeBuffer - out of bound access: index {index} - capacity {_capacity}");
#endif
                    return ref dense[index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void insert(T val)
        {
            if (!has(val))
            {
                var index = val.ToUint();
                if (index >= _capacity)
                    throw new Exception("Out of bounds exception");

                dense[_size]  = val;
                sparse[index] = _size;
                ++_size;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void remove(T val)
        {
            if (has(val))
            {
                var index = val.ToUint();
                dense[sparse[index]]              = dense[_size - 1];
                sparse[dense[_size - 1].ToUint()] = sparse[index];
                --_size;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void intersect(in SparseSet<T> otherSet)
        {
            for (uint i = 0; i < _size; i++)
            {
                if (otherSet.has(this[i]) == false)
                    this.remove(this[i]);
            }
        }

        int  _size     = 0; //Current size (number of elements)
        uint _capacity = 0;
    };

    public interface IConvertToUInt
    {
        uint ToUint();
    }
}