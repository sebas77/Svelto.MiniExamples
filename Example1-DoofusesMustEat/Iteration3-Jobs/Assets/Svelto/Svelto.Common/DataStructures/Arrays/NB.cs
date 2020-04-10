using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// NB stands for NB
    /// NativeBuffers are current designed to be used inside Jobs. They wrap an EntityDB array of components
    /// but do not track it. Hence it's meant to be used temporary and locally as the array can become invalid
    /// after a submission of entities.
    ///
    /// ToDo: Sentinel to invalidate the array if a submission happens
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NB<T>:IBuffer<T> where T:unmanaged
    {
        public void Dispose()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }
        
        public unsafe NB(T* array, uint count) : this()
        {
            _ptr = new IntPtr(array);
            _count = count;
        }

        public NB(T[] array, uint count) : this()
        {
            _handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            _ptr    = _handle.AddrOfPinnedObject();
            
            _count = count;
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

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (index >= _count)
                        throw new Exception("NativeBuffer - out of bound access");
#endif                    
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
#if DEBUG && !PROFILE_SVELTO
                    if (index >= _count)
                        throw new Exception("NativeBuffer - out of bound access");
#endif                    
                    
                    return ref ((T*) _ptr)[index];
                    //return ref Unsafe.AsRef<T>(Unsafe.Add<T>((void*) _ptr, (int) index));
                }
            }
        }

        GCHandle _handle;
        uint _count;
#if UNITY_COLLECTIONS        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _ptr;
    }
}
