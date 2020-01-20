using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    public struct NativeBuffer<T>:IBuffer<T> where T:unmanaged
    {
        public NativeBuffer(T[] array) : this()
        {
            Set(array);
        }
        
        public NativeBuffer(GCHandle alloc, uint count):this()
        {
            Set(alloc, count);
        }
        
        public void Set(T[] array)
        {
            _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            _ptr = _handle.AddrOfPinnedObject();
        }

        public void Set<Buffer>(Buffer array) where Buffer : IBuffer<T>
        {
            _handle = array.Pin();
            _ptr = _handle.AddrOfPinnedObject();
        }

        public void Set(GCHandle handle, uint count)
        {
            _handle = handle;
            _ptr = _handle.AddrOfPinnedObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint initialSize)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom<TBuffer>(TBuffer array, uint startIndex, uint size) where TBuffer:IBuffer<T>
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] destination, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ICollection<T> source)
        {
            throw new NotImplementedException(); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint startIndex, uint count)
        {
            throw new NotImplementedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            throw new NotImplementedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item, int count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, int count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            throw new NotImplementedException();
        }

        public IntPtr ToNativeArray() { return _ptr; }

        public GCHandle Pin() { return _handle; }

        public void Dispose()
        {
            _handle.Free();
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return ref ((T*) _ptr)[index];
                    //return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*) _ptr, (int) index));
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
                    return ref ((T*) _ptr)[index];
                    //return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*) _ptr, (int) index));
                }
            }
        }

        GCHandle _handle;
#if ENABLE_BURST_AOT        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        IntPtr _ptr;
    }
}
