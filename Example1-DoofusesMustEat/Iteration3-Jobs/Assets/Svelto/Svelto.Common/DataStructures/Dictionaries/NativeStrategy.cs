using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
        IBuffer<T> buffer;
        NB<T> realBuffer;

        public NativeStrategy(uint size):this()
        {
            Alloc(size);
        }

        public void Alloc(uint newCapacity)
        {
            DBC.Common.Check.Require(buffer == null || buffer.ToNativeArray(out _) == IntPtr.Zero, "can't alloc an already allocated buffer");

            var   realBuffer = MemoryUtilities.Alloc((uint) (newCapacity * MemoryUtilities.SizeOf<T>()), Allocator.Persistent);
            NB<T> b          = new NB<T>(realBuffer, newCapacity);
            buffer = b;
            this.realBuffer = b;
        }

        public void Clear() => buffer.Clear();
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

        public IntPtr ToNativeArray() { return realBuffer.ToNativeArray(out _); } //todo: this should be internal 
        public IBuffer<T> ToBuffer() { return buffer; }
        
        public int capacity => realBuffer.capacity;

        public void Resize(uint newCapacity)
        {
            DBC.Common.Check.Require(newCapacity > 0, "Resize requires a size greater than 0");
            DBC.Common.Check.Require(newCapacity > capacity, "can't resize to a smaller size");

            var pointer = buffer.ToNativeArray(out _);
            var sizeOf = MemoryUtilities.SizeOf<T>();
            pointer = MemoryUtilities.Realloc(pointer, (uint) (capacity * sizeOf), (uint) (newCapacity * sizeOf), 
                                              Allocator.Persistent);
            NB<T> b = new NB<T>(pointer, newCapacity);
            buffer = b;
            realBuffer = b;
        }

        public void Dispose()
        {
            if (buffer != null && buffer.ToNativeArray(out _) != IntPtr.Zero)
                MemoryUtilities.Free(buffer.ToNativeArray(out _), Allocator.Persistent);
            else
                if (buffer == null)
                    Svelto.Console.LogWarning($"trying to dispose a never allocated buffer. Type held: {typeof(T)}");
                else
                   if (buffer.ToNativeArray(out _) != IntPtr.Zero)
                       Svelto.Console.LogWarning($"trying to dispose disposed buffer. Type held: {typeof(T)}");
            
            realBuffer = default;
            buffer = realBuffer;
        }
    }
}