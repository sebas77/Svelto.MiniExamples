#if UNITY_2017_2_OR_NEWER
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Svelto.DataStructures
{
    public struct SimpleNativeArray : IDisposable
    {
        IntPtr _ptr;
        public uint length;

        public void Alloc<T>(uint newLength) where T : unmanaged
        {
            unsafe
            {
#if DEBUG
                if(_ptr != default) throw new Exception("SimpleNativeArray: Alloc called twice");
#endif
                int elemSize = UnsafeUtility.SizeOf<T>();
                int elemAlign = UnsafeUtility.AlignOf<T>();
                _ptr = (IntPtr)UnsafeUtility.Malloc(newLength * elemSize, elemAlign, Allocator.Persistent);
                length = newLength;
            }
        }

        public ref T Get<T>(uint index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG
                if (_ptr == IntPtr.Zero) throw new Exception("SimpleNativeArray: null-access");
                if(index >= length) throw new Exception("SimpleNativeArray: out of bound index");
#endif
                return ref ((T *)_ptr)[index];
            }
        }

        public void Set<T>(uint index, in T value) where T : unmanaged
        {
            Get<T>(index) = value;
        }

        public unsafe void Dispose()
        {
            if (_ptr != IntPtr.Zero)
            {
                UnsafeUtility.Free((void*)_ptr, Allocator.Persistent);
                _ptr = IntPtr.Zero;
            }
        }
    }
}

#endif