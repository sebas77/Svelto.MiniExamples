#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Svelto.Common;

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

                _array.CopyTo(0, array, 0, (uint)_array.capacity);

                return array;
            }
        }

        NBInternal<T> _array;
    }

    /// <summary>
    /// NB stands for NativeBuffer
    /// 
    /// NativeBuffers were initially mainly designed to be used inside Unity Jobs. They wrap an EntityDB array of components
    /// but do not track it. Hence, it's meant to be used temporary and locally as the array can become invalid
    /// after a submission of entities. However, they cannot be used as ref struct
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
    readonly struct NBInternal<T> : IBuffer<T>where T : struct
    {
        /// <summary>
        /// Note: static constructors are NOT compiled by burst as long as there are no static fields in the struct
        /// </summary>
        static NBInternal()
        {
#if ENABLE_DEBUG_CHECKS
            if (TypeCache<T>.isUnmanaged == false)
                throw new Exception("NativeBuffer (NB) supports only unmanaged types");
#endif
        }

        public NBInternal(IntPtr array, uint capacity): this()
        {
            _ptr = array;
            _capacity = capacity;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            //    using (_threadSentinel.TestThreadSafety())
            {
                for (int i = 0; i < count; i++)
                {
                    destination[i] = this[i];
                }
            }
        }

        public void Clear()
        {
            //  using (_threadSentinel.TestThreadSafety())
            {
                MemoryUtilities.MemClear<T>(_ptr, _capacity);
            }
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
                    //     using (_threadSentinel.TestThreadSafety())
                    {
                        return ref Unsafe.AsRef<T>((void*)(_ptr + (int)index * MemoryUtilities.SizeOf<T>()));
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
                    //                  using (_threadSentinel.TestThreadSafety())
                    {
                        return ref Unsafe.AsRef<T>((void*)(_ptr + index * MemoryUtilities.SizeOf<T>()));
                    }
                }
            }
        }
        
        public static implicit operator NB<T>(NBInternal<T> proxy) => new NB<T>(proxy);
        public static implicit operator NBInternal<T>(NB<T> proxy) => new NBInternal<T>(proxy.ToNativeArray(out var capacity), (uint)capacity);

        readonly uint _capacity;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _ptr;

//        readonly Sentinel _threadSentinel;

//        //Todo: this logic is not completed yet, WIP
//        public NBParallelReader AsReader()
//        {
//            return new NBParallelReader(this, new Sentinel(this._ptr, Sentinel.readFlag));
//        }
//
//        public NBParallelWriter AsWriter()
//        {
//            return new NBParallelWriter(this, new Sentinel(this._ptr, Sentinel.writeFlag));
//        }
//
//        public struct NBParallelReader
//        {
//            public NBParallelReader(NB<T> nb, Sentinel sentinel)
//            {
//                throw new NotImplementedException();
//            }
//        }
//        
//        public struct NBParallelWriter
//        {
//            public NBParallelWriter(NB<T> nb, Sentinel sentinel)
//            {
//                throw new NotImplementedException();
//            }
//        }
    }

    /// <summary>
    /// Note: this struct should be ref, however with jobs is a common pattern to use NB as a field of a struct. This pattern should be replaced
    /// with the introduction of new structs that can be hold but must be requested through some contracts like:
    /// AsReader, AsWriter, AsReadOnly, AsParallelReader, AsParallelWriter and so on. In this way NB can keep track about how the buffer is used
    /// and can throw exceptions if the buffer is used in the wrong way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly /*ref*/ struct NB<T> where T : struct
    {
        readonly NBInternal<T> _bufferImplementation;

        internal NB(NBInternal<T> nbInternal)
        {
            _bufferImplementation = nbInternal;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            _bufferImplementation.CopyTo(sourceStartIndex, destination, destinationStartIndex, count);
        }

        public void Clear()
        {
            _bufferImplementation.Clear();
        }

        public int capacity => _bufferImplementation.capacity;

        public bool isValid => _bufferImplementation.isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray(out int capacity)
        {
            return _bufferImplementation.ToNativeArray(out capacity);
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _bufferImplementation[index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif

                return ref _bufferImplementation[index];
            }
        }
    }
}