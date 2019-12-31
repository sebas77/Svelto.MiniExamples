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
        void Set<Buffer>(Buffer array) where Buffer:IBuffer<T>;
        void Set(GCHandle handle, uint count);
        void Allocate(uint             initialSize);
        void Resize(uint               initialSize);
        void CopyFrom<TBuffer>(TBuffer array,  uint startIndex,       uint size) where TBuffer : IBuffer<T>;
        void CopyFrom(T[]              source, uint sourceStartIndex, uint destinationStartIndex, uint size);
        void CopyFrom(ICollection<T>   source);
        void CopyTo(T[]                destination, uint sourceStartIndex, uint destinationStartIndex, uint size);
        uint length { get; }
        void Clear(uint startIndex, uint count);
        void Clear();
        void Insert(int   index, in T item, int count);
        void RemoveAt(int index, int  count);
        T[]  ToManagedArray();
        IntPtr ToNativeArray();
        GCHandle Pin();
    }
}