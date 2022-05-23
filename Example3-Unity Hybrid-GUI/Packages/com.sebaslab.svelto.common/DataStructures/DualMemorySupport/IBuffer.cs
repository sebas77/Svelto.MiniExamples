using System;

namespace Svelto.DataStructures
{
    public interface IBufferBase<T>
    {
    }

    public interface IBuffer<T>:IBufferBase<T>
    {
        void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count);
        void Clear();
        
        T[]    ToManagedArray();
        IntPtr ToNativeArray(out int capacity);

        int capacity { get; }
        bool isValid  { get; }
    }
}