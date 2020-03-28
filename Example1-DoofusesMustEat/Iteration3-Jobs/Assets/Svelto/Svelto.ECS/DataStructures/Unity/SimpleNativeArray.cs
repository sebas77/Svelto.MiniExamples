#if UNITY_ECS
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Svelto.DataStructures
{
    public struct SimpleNativeArray : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] unsafe UnsafeList* _list;
#if DEBUG && !PROFILE_SVELTO
        int hashType;
#endif

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return (uint) _list->Length;
                }
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
                rtnStruc._list = UnsafeList.Create(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>()
                                                 , (int) newLength, allocator);

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
                if (index >= _list->Length)
                    throw new Exception($"SimpleNativeArray: out of bound access, index {index} count {count}");
#endif
                T* buffer = (T*) _list->Ptr;
                return ref buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : unmanaged => Get<T>(index) = value;

        public unsafe void Dispose()
        {
            if (_list != null)
            {
                UnsafeList.Destroy(_list);
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
                if (_list->Length == _list->Capacity)
                    Realloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), (int) ((_list->Capacity + 1) * 1.5f)
                          , _list->Allocator);

                T* buffer = (T*) _list->Ptr;
                buffer[_list->Length] = item;
                _list->Length         = _list->Length + 1;
            }
        }

        void Realloc(int sizeOf, int alignOf, int capacity, Allocator allocator)
        {
            unsafe
            {
                void* newPointer = null;

                if (capacity > 0)
                {
                    var bytesToMalloc = sizeOf * capacity;
                    newPointer = UnsafeUtility.Malloc(bytesToMalloc, alignOf, allocator);

                    if (_list->Length > 0)
                    {
                        var itemsToCopy = capacity > _list->Length ? _list->Length : capacity;
                        var bytesToCopy = itemsToCopy * sizeOf;
                        UnsafeUtility.MemCpy(newPointer, _list->Ptr, bytesToCopy);
                    }
                }

                UnsafeUtility.Free(_list->Ptr, allocator);

                _list->Ptr      = newPointer;
                _list->Capacity = capacity;
                _list->Length   = capacity > _list->Length ? _list->Length : capacity;
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

#endif