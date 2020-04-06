using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    /// <summary>
    /// Burst friendly RingBuffer on steroid:
    /// it can: Enqueue/Dequeue, it wraps if there is enough space after dequeuing
    /// It resizes if there isn't enough space left.
    /// It's a "bag, you can queue and dequeue any T. Just be sure that you dequeue what you queue! No check on type
    /// is done.
    /// You can reserve a position in the queue to update it later.
    /// The datastructure is a struct and it's "copyable" 
    /// </summary>
    public struct NativeRingBuffer : IDisposable
    {
#if ENABLE_BURST_AOT        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            unsafe
            {
                if (_list == null || _list->ptr == null)
                    return true;
            }

            return count == 0;
        }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (_list == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif
                    
                    return _list->size;
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
#if DEBUG && !PROFILE_SVELTO
                    if (_list == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif

                    return _list->capacity;
                }
            }
        }

        public NativeRingBuffer(Common.Allocator allocator)
        {
            unsafe 
            {
                UnsafeArray* listData =
                    (UnsafeArray*) MemoryUtilities.Alloc(MemoryUtilities.SizeOf<UnsafeArray>()
                                                       , MemoryUtilities.AlignOf<UnsafeArray>(), allocator);
                MemoryUtilities.MemClear((IntPtr) listData, MemoryUtilities.SizeOf<UnsafeArray>());

                listData->allocator = allocator;

                _list = listData;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            if (_list != null)
            {
                _list->Dispose();
                _list = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReserveEnqueue<T>(out UnsafeArrayIndex index) where T:unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                if (_list->space - sizeof(T) < 0)
                    _list->Realloc(MemoryUtilities.AlignOf<int>(), (uint) ((_list->capacity + sizeof(T)) * 1.5f));
                
                return ref _list->Reserve<T>(out index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(in T item) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                var sizeOf = Unsafe.SizeOf<T>();
                if (_list->space - sizeOf < 0)
                    _list->Realloc(MemoryUtilities.AlignOf<int>(), (uint) ((_list->capacity + sizeOf) * 1.5f));

                _list->Write(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                _list->Clear();
            }
        }

        public T Dequeue<T>() where T : struct
        {
            unsafe 
            {
                return _list->Read<T>();
            }
        }

        public ref T AccessReserved<T>(UnsafeArrayIndex reserverIndex) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                return ref _list->AccessReserved<T>(reserverIndex);
            }
        }
    }
}
