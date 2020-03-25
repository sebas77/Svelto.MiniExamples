using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    public struct NativeBuffer<T>:IBuffer<T> where T:unmanaged
    {
        public void Dispose()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }
        
        public unsafe NativeBuffer(T* array) : this()
        {
            _ptr = new IntPtr(array);
        }

        public NativeBuffer(T[] array) : this()
        {
            Set(array);
        }
        
        public void Set(T[] array)
        {
            _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            _ptr = _handle.AddrOfPinnedObject();
        }

        public void CopyFrom<TBuffer>(TBuffer array, uint startIndex, uint size) where TBuffer:IBuffer<T>
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(T[] source, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] destination, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(ICollection<T> source)
        {
            throw new NotImplementedException(); 
        }

        public void Clear(uint startIndex, uint count)
        {
            throw new NotImplementedException();
        }
        
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void UnorderedRemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T[] ToManagedArray()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray() { return _ptr; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GCHandle Pin() { return _handle; }

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
