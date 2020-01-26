using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    public interface IBuffer<T>:IDisposable
    {
        ref T this[uint index] { get; }
        ref T this[int index] { get; }
        
        void Set(T[]                   array);
        void CopyFrom<TBuffer>(TBuffer array,  uint startIndex,       uint size) where TBuffer : IBuffer<T>;
        void CopyFrom(T[]              source, uint sourceStartIndex, uint destinationStartIndex, uint size);
        void CopyFrom(ICollection<T>   source);
        void CopyTo(T[]                destination, uint sourceStartIndex, uint destinationStartIndex, uint size);
        void Clear(uint startIndex, uint count);
        void Clear();
        void UnorderedRemoveAt(int index);
        T[]  ToManagedArray();
        IntPtr ToNativeArray();
        GCHandle Pin();
    }
}