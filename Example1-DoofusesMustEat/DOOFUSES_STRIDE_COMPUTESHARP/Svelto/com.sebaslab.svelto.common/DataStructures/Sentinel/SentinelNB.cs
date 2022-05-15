using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.Common.DataStructures
{
    /// <summary>
    /// internal structure just for the sake of creating the sentinel dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct SentinelNB<T> : IBuffer<T> where T : struct
    {
        public SentinelNB(IntPtr array, uint capacity) : this()
        {
            _ptr      = array;
            _capacity = capacity;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                destination[i] = this[i];
            }
        }

        public void Clear()
        {
            MemoryUtilities.MemClear<T>(_ptr, _capacity);
        }

        public T[] ToManagedArray()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray(out int capacity)
        {
            capacity = (int)_capacity;
            return _ptr;
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)_capacity;
        }

        public bool isValid => _ptr != IntPtr.Zero;

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if ENABLE_DEBUG_CHECKS
                        if (index >= _capacity)
                            throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                    {
                        var     size  = MemoryUtilities.SizeOf<T>();
                        ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + (int)(index * size)));
                        return ref asRef;
                    }
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
#if ENABLE_DEBUG_CHECKS
                        if (index < 0 || index >= _capacity)
                            throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                    {
                        var     size  = MemoryUtilities.SizeOf<T>();
                        ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + index * size));
                        return ref asRef;
                    }
                }
            }
        }

        readonly uint _capacity;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _ptr;

        //I can't make the _threadSentinel shareable, so I have to assume that this datastructure is used in
        //parallel only with mechanism similar to the DOTS job strategy
    }
}