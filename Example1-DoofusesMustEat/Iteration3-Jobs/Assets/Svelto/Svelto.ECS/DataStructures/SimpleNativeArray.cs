using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures
{
    public struct SimpleNativeArray : IDisposable
    {
#if ENABLE_BURST_AOT        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int hashType;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>() where T:unmanaged
        {
            unsafe
            {
                return (uint) (_list->size / sizeof(T));
            }
        }

        public static SimpleNativeArray Alloc<T>(Allocator allocator, uint newLength = 0) where T : unmanaged
        {
            unsafe
            {
                var rtnStruc = new SimpleNativeArray();
#if DEBUG && !PROFILE_SVELTO
                rtnStruc.hashType = typeof(T).GetHashCode();
#endif
                var sizeOf  = MemoryUtilities.SizeOf<T>();
                var alignOf = MemoryUtilities.AlignOf<T>();

                UnsafeArray* listData =
                    (UnsafeArray*) MemoryUtilities.Alloc(MemoryUtilities.SizeOf<UnsafeArray>()
                                                        , MemoryUtilities.AlignOf<UnsafeArray>(), allocator);
                MemoryUtilities.MemClear((IntPtr) listData, MemoryUtilities.SizeOf<UnsafeArray>());

                listData->allocator = allocator;

                if (newLength > 0)
                    listData->Realloc((uint) alignOf, (uint) (newLength * sizeOf));

                MemoryUtilities.MemClear((IntPtr) listData->ptr, listData->capacity * sizeOf);

                rtnStruc._list = listData;

                return rtnStruc;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (hashType != typeof(T).GetHashCode())
                    throw new Exception("SimpleNativeArray: not except type used");
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
                if (index >= Count<T>())
                    throw new Exception($"SimpleNativeArray: out of bound access, index {index} count {Count<T>()}");
#endif
                T* buffer = (T*) _list->ptr;
                return ref buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : unmanaged => Get<T>(index) = value;

        public unsafe void Dispose()
        {
            if (_list != null)
            {
                _list->Dispose();
                _list = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T item) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (hashType != typeof(T).GetHashCode())
                    throw new Exception("SimpleNativeArray: not except type used");
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                if (_list->space == 0)
                    _list->Realloc((uint) MemoryUtilities.AlignOf<T>(), (uint) ((_list->capacity + 1) * 1.5f));

                _list->WriteUnaligned(item);
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
    }
}
