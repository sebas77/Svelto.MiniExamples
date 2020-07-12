using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct ManagedStrategy<T> : IBufferStrategy<T>
    {
        IBuffer<T> buffer;
        MB<T> realBuffer;

        public ManagedStrategy(uint size):this()
        {
            Alloc(size, Allocator.None);
        }

        public void Alloc(uint size, Allocator nativeAllocator)
        {
            MB<T> b = new MB<T>();
            b.Set(new T[size]);
            buffer = b;
            this.realBuffer = b;
        }

        public int capacity => buffer.capacity;

        public void Resize(uint newCapacity)
        {
            DBC.Common.Check.Require(newCapacity > 0, "Resize requires a size greater than 0");
            
            var realBuffer = buffer.ToManagedArray();
            Array.Resize(ref realBuffer, (int) newCapacity);
            MB<T> b = new MB<T>();
            b.Set(realBuffer);
            buffer = b;
            this.realBuffer = b;
        }

        public void Clear() => realBuffer.Clear();
        public void FastClear() => realBuffer.FastClear();

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref realBuffer[index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref realBuffer[index];
        }

        public IntPtr ToNativeArray() => throw new NotImplementedException();
        public IBuffer<T> ToBuffer() => buffer;

        public void Dispose() {  }
    }
}