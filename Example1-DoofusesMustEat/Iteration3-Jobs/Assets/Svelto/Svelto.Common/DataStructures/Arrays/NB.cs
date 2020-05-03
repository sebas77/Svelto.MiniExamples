using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    /// <summary>
    /// NB stands for NB
    /// NativeBuffers are current designed to be used inside Jobs. They wrap an EntityDB array of components
    /// but do not track it. Hence it's meant to be used temporary and locally as the array can become invalid
    /// after a submission of entities.
    ///
    /// NB are wrappers of arrays. Are not meant to resize or free
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NB<T>:IBuffer<T> where T:unmanaged
    {
        public NB(IntPtr array, uint count, uint capacity) : this()
        {
#if DEBUG && !PROFILE_SVELTO
            if (count > capacity)
                throw new Exception("count can't be more than capacity");
#endif 
            _ptr = array;
            _capacity = capacity;
            _count = count;
        }

        public void Set(IntPtr array, uint count, uint capacity)
        {
            _ptr      = array;
            _capacity = capacity;
            _count    = count;
        }

        public void CopyFrom<TBuffer>(TBuffer array, uint startIndex, uint size) where TBuffer:IBuffer<T>
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(T[] source, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint size)
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

        public uint capacity
        {
            get => _capacity;
        }

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

        uint _count;
        uint _capacity;
#if UNITY_COLLECTIONS
        //todo can I remove this from here? it should be used outside
        [Unity.Burst.NoAlias]
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        IntPtr _ptr;

        public NB<T> AsReader() { return this; }
        public NB<T> AsWriter() { return this; }
    }
}
