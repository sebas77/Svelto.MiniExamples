#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Common.DataStructures;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct ComputeSharpBuffer<T>:IBuffer<T> where T:struct
    {
        /// <summary>
        /// Note: static constructors are NOT compiled by burst as long as there are no static fields in the struct
        /// </summary>
        static ComputeSharpBuffer()
        {
#if ENABLE_DEBUG_CHECKS            
            if (TypeType.isUnmanaged<T>() == false)
                throw new Exception("NativeBuffer (NB) supports only unmanaged types");
#endif            
        }
        
        public ComputeSharpBuffer(IntPtr array, uint capacity) : this()
        {
            _ptr      = array;
            _capacity = capacity;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            using (_threadSentinel.TestThreadSafety())
            {
                for (int i = 0; i < count; i++)
                {
                    destination[i] = this[i];
                }
            }
        }
        public void Clear()
        {
            using (_threadSentinel.TestThreadSafety())
            {
                MemoryUtilities.MemClear<T>(_ptr, _capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray(out int capacity)
        {
            capacity = (int) _capacity; return _ptr; 
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int) _capacity;
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
                    using (_threadSentinel.TestThreadSafety())
                    {
                        ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + (int)index * SIZE));
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
                    using (_threadSentinel.TestThreadSafety())
                    {
                        ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + index * SIZE));
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
        
        readonly Sentinel _threadSentinel;

        static readonly int SIZE = MemoryUtilities.SizeOf<T>();
    }
}
