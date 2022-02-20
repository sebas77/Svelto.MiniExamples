#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Common.DataStructures;

namespace Svelto.ECS.DataStructures
{
    /// <summary>
    ///     Burst friendly RingBuffer on steroid:
    ///     it can: Enqueue/Dequeue, it wraps around if there is enough space after dequeuing
    ///     It resizes if there isn't enough space left.
    ///     It's a "bag", you can queue and dequeue any type and mix them. Just be sure that you dequeue what you queue! No check on type
    ///     is done.
    ///     You can reserve a position in the queue to update it later.
    ///     The datastructure is a struct and it's "copiable"
    ///     I eventually decided to call it NativeBag and not NativeBag because it can also be used as
    ///     a preallocated memory pool where any kind of T can be stored as long as T is unmanaged
    /// </summary>
    public struct NativeBag : IDisposable
    {
        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    BasicTests();
                    
#if ENABLE_DEBUG_CHECKS                    
                    using (_threadSentinel.TestThreadSafety())
                    {
#endif                    
                        return _queue->size;
#if ENABLE_DEBUG_CHECKS                        
                    }
#endif                    
                }
            }
        }

        public uint capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    BasicTests();
                    
#if ENABLE_DEBUG_CHECKS                    
                    using (_threadSentinel.TestThreadSafety())
                    {
#endif                    
                        return _queue->capacity;
#if ENABLE_DEBUG_CHECKS                        
                    }
#endif                    
                }
            }
        }

        public NativeBag(Allocator allocator):this()
        {
            unsafe
            {
                var listData = (UnsafeBlob*)MemoryUtilities.Alloc<UnsafeBlob>((uint)1, allocator);

                listData->allocator = allocator;
                _queue              = listData;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            unsafe
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS                
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    if (_queue == null || _queue->ptr == null)
                        return true;
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }

            return count == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            if (_queue != null)
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _queue->Dispose();
                    MemoryUtilities.Free((IntPtr)_queue, _queue->allocator);
                    _queue = null;

#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReserveEnqueue<T>
            (out UnsafeArrayIndex index)
            where T : struct //should be unmanaged, but it's not due to Svelto.ECS constraints.
        {
            unsafe
            {
                BasicTests();

                var sizeOf = MemoryUtilities.SizeOf<T>();
                
#if ENABLE_DEBUG_CHECKS                
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    if (_queue->availableSpace - sizeOf < 0)
                    {
                        _queue->Grow<T>();
                    }

                    return ref _queue->Reserve<T>(out index);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>
            (in T item) where T : struct //should be unmanaged, but it's not due to Svelto.ECS constraints.
        {
            unsafe
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    var sizeOf = MemoryUtilities.SizeOf<T>();
                    if (_queue->availableSpace - sizeOf < 0)
                    {
                        _queue->Grow<T>();
                    }

                    _queue->Enqueue(item);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _queue->Clear();
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        public T Dequeue<T>() where T : struct //should be unmanaged, but it's not due to Svelto.ECS constraints.
        {
            unsafe
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    return _queue->Dequeue<T>();
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        public ref T AccessReserved<T>(UnsafeArrayIndex reservedIndex) where T : struct //should be unmanaged, but it's not due to Svelto.ECS constraints.
        {
            unsafe
            {
                BasicTests();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    return ref _queue->AccessReserved<T>(reservedIndex);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        [Conditional("ENABLE_DEBUG_CHECKS")]
        unsafe void BasicTests()
        {
            if (_queue == null)
                throw new Exception("SimpleNativeArray: null-access");
        }
        
        readonly Sentinel _threadSentinel;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeBlob* _queue;
    }
}