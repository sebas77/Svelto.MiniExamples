using System;

namespace Svelto.DataStructures
{
    public interface IBuffer<T>
    {
        ref T this[uint index] { get; }
        ref T this[int index] { get; }
        
        void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint size);
        void Clear();
        
        T[]  ToManagedArray();
        IntPtr ToNativeArray();

        uint capacity { get; }
        uint count { get; }
    }
}