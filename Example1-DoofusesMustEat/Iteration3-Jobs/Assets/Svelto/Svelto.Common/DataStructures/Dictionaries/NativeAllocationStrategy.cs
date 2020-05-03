using System;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public class NativeAllocationStrategy<T> : IAccess<T> where T : unmanaged
    {
        IBuffer<T> buffer;

        public void Alloc(uint size)
        {
            DBC.Common.Check.Require(buffer == null || buffer.ToNativeArray() == IntPtr.Zero, "can't alloc an already allocated buffer");

            var   realBuffer = MemoryUtilities.Alloc((uint) (size * MemoryUtilities.SizeOf<T>()), Allocator.Persistent);
            NB<T> b          = new NB<T>(realBuffer, size, size);
            buffer = b;
        }

        public IBuffer<T> values => buffer;

        public uint capacity => buffer.capacity;

        public void Resize(uint size)
        {
            DBC.Common.Check.Require(size > 0, "Resize requires a size greater than 0");
            DBC.Common.Check.Require(size > capacity, "can't resize to a smaller size");

            var realBuffer = buffer.ToNativeArray();
            MemoryUtilities.Realloc(ref realBuffer, (uint) (capacity * MemoryUtilities.SizeOf<T>())
                                  , (uint) (size * MemoryUtilities.SizeOf<T>()), Allocator.Persistent);
            NB<T> b = default;
            b.Set(realBuffer, size, size);
            buffer = b;
        }

        public void Dispose()
        {
            DBC.Common.Check.Require(buffer != null && buffer.ToNativeArray() != IntPtr.Zero, "trying to dispose disposed buffer");
            
            MemoryUtilities.Free(buffer.ToNativeArray(), Allocator.Persistent);
        }
    }
}