#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Common.DataStructures;

namespace Svelto.DataStructures
{
    sealed class NBDebugProxy<T> where T : struct
    {
        public NBDebugProxy(NB<T> array)
        {
            this._array = array;
        }

        public T[] Items
        {
            get
            {
                T[] array = new T[_array.capacity];
                
                _array.CopyTo(0, array, 0, (uint) _array.capacity);

                return array;
            }
        }
        
        NB<T> _array;
    }

    /// <summary>
    /// NB stands for NativeBuffer
    /// 
    /// NativeBuffers were initially mainly designed to be used inside Unity Jobs. They wrap an EntityDB array of components
    /// but do not track it. Hence it's meant to be used temporary and locally as the array can become invalid
    /// after a submission of entities. However they cannot be used as ref struct
    ///
    /// ------> NBs are wrappers of native arrays. Are not meant to resize or be freed
    ///
    /// NBs cannot have a count, because a count of the meaningful number of items is not tracked.
    /// Example: an NB could be initialized with a size 10 and count 0. Then the buffer is used to fill entities
    /// but the count will stay zero. It's not the NB responsibility to track the count
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    
    [DebuggerTypeProxy(typeof(NBDebugProxy<>))]
    public struct NB<T>:IBuffer<T> where T:struct
    {
        /// <summary>
        /// Note: static constructors are NOT compiled by burst as long as there are no static fields in the struct
        /// </summary>
        static NB()
        {
#if ENABLE_DEBUG_CHECKS            
            if (TypeType.isUnmanaged<T>() == false)
                throw new Exception("NativeBuffer (NB) supports only unmanaged types");
#endif            
        }
        
        public NB(IntPtr array, uint capacity) : this()
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

        public T[] ToManagedArray()
        {
            throw new NotImplementedException();
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
                        ref var asRef = ref Unsafe.AsRef<T>((void*)(_ptr + (int)(index * SIZE)));
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

        //Todo: this logic is not completed yet, WIP
        public NBParallelReader AsReader()
        {
            return new NBParallelReader(this, new Sentinel(this._ptr, Sentinel.readFlag));
        }

        public NBParallelWriter AsWriter()
        {
            return new NBParallelWriter(this, new Sentinel(this._ptr, Sentinel.writeFlag));
        }

        public struct NBParallelReader
        {
            public NBParallelReader(NB<T> nb, Sentinel sentinel)
            {
                throw new NotImplementedException();
            }
        }
        
        public struct NBParallelWriter
        {
            public NBParallelWriter(NB<T> nb, Sentinel sentinel)
            {
                throw new NotImplementedException();
            }
        }
        
        static readonly int SIZE = MemoryUtilities.SizeOf<T>();
    }
}
