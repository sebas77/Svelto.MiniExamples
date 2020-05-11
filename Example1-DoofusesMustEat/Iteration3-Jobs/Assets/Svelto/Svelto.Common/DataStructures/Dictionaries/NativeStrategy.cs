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

        public bool IsUnmanaged => true;

        public void Alloc(uint size)
        {
            DBC.Common.Check.Require(buffer == null || buffer.ToNativeArray(out _) == IntPtr.Zero, "can't alloc an already allocated buffer");

            var   realBuffer = MemoryUtilities.Alloc((uint) (size * MemoryUtilities.SizeOf<T>()), Allocator.Persistent);
            NB<T> b          = new NB<T>(realBuffer, size);
            buffer = b;
            this.realBuffer = b;
        }

        public void Clear() => buffer.Clear();

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

        public IntPtr ToNativeArray() { return buffer.ToNativeArray(out _); }
        public IBuffer<T> ToBuffer() { return buffer; }
        
        public int capacity => realBuffer.capacity;

        public void Resize(uint size)
        {
            DBC.Common.Check.Require(size > 0, "Resize requires a size greater than 0");
            DBC.Common.Check.Require(size > capacity, "can't resize to a smaller size");

            var realBuffer = buffer.ToNativeArray(out _);
            MemoryUtilities.Realloc(ref realBuffer, (uint) (capacity * MemoryUtilities.SizeOf<T>())
                                  , (uint) (size * MemoryUtilities.SizeOf<T>()), Allocator.Persistent);
            NB<T> b = new NB<T>();
            b.Set(realBuffer, size);
            buffer = b;
            this.realBuffer = b;
        }

        public void Dispose()
        {
            DBC.Common.Check.Require(buffer == null || buffer.ToNativeArray(out _) != IntPtr.Zero, "trying to dispose disposed buffer");
            
            MemoryUtilities.Free(buffer.ToNativeArray(out _), Allocator.Persistent);
        }
    }
}