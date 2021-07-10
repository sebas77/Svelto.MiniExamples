using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public ref struct LocalSparseSet<T> where T: unmanaged, IConvertToUInt
    {
        readonly unsafe T*    dense; //Dense set of elements
        readonly unsafe uint* sparse; //Map of elements to dense set indices

        uint          _size; //Current size (number of elements)
        readonly uint _capacity;

        static LocalSparseSet()
        {
            DBC.Common.Check.Require(MemoryUtilities.SizeOf<T>() == MemoryUtilities.SizeOf<int>(), "LocalSparseSet can be used only with types blittable to int");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe LocalSparseSet(uint capacity, T *    spanDense, uint * spanSparse):this()
        {
            _size = 0;
            MemoryUtilities.MemClear<T>((IntPtr) spanDense, capacity);
            MemoryUtilities.MemClear<uint>((IntPtr) spanSparse, capacity);
            sparse    = spanSparse;
            dense     = (T*) spanDense;
            _capacity = capacity;
        }

        public uint size => _size;

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
                        throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {_capacity}");
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
                        throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {_capacity}");
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
                unsafe
                {
                    var index = val.ToUint();
                    if (index >= _capacity)
                        throw new Exception("Out of bounds exception");

                    dense[_size]  = val;
                    sparse[index] = _size;
                    ++_size;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void remove(T val)
        {
            if (has(val))
            {
                unsafe
                {
                    var index = val.ToUint();
                    dense[sparse[index]]              = dense[_size - 1];
                    sparse[dense[_size - 1].ToUint()] = sparse[index];
                    --_size;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void intersect(in LocalSparseSet<T> otherSet)
        {
            for (uint i = 0; i < size; i++)
            {
                if (otherSet.has(this[i]) == false)
                    this.remove(this[i]);
            }
        }
    };
}