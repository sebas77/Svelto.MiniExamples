using System;

namespace Svelto.DataStructures
{
    public interface IBufferStrategy<T>: IDisposable
    {
        int capacity { get; }

        void Alloc(uint size);
        void Resize(uint newCapacity);
        void Clear();
        
        ref T this[uint index] { get ; }
        ref T this[int index] { get ; }
        
        IntPtr ToNativeArray();
        IBuffer<T> ToBuffer();
        void FastClear();
    }
}