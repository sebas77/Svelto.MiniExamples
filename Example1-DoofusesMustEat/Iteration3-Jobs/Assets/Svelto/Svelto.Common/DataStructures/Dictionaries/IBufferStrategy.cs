using System;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public interface IBufferStrategy<T>: IDisposable
    {
        int capacity { get; }

        void Alloc(uint size, Allocator nativeAllocator);
        void Resize(uint newCapacity);
        void Clear();
        
        ref T this[uint index] { get ; }
        ref T this[int index] { get ; }
        
        //ToDo: somethihg must be done for this
        IntPtr ToNativeArray();
        //ToDo: somethihg must be done for this
        IBuffer<T> ToBuffer();
    }
}