using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DBC.Common;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
        Allocator _nativeAllocator;
        NB<T> realBuffer;
#if UNITY_COLLECTIONS        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        IntPtr _buffer;
        
        public NativeStrategy(uint size, Allocator nativeAllocator):this()
        {
            Alloc(size, nativeAllocator);
        }

        public void Alloc(uint newCapacity, Allocator nativeAllocator)
        {
            _nativeAllocator = nativeAllocator;
            
            Check.Require(this.realBuffer.ToNativeArray(out _) == IntPtr.Zero, "can't alloc an already allocated buffer");

            var   realBuffer = MemoryUtilities.Alloc((uint) (newCapacity * MemoryUtilities.SizeOf<T>()), _nativeAllocator);
            NB<T> b          = new NB<T>(realBuffer, newCapacity);
            IBuffer<T> buffer = b;

            _buffer = GCHandle.ToIntPtr(GCHandle.Alloc(buffer, GCHandleType.Normal));
                    
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

        public IntPtr ToNativeArray() { return realBuffer.ToNativeArray(out _); } //todo: this should be internal
        
        public IBuffer<T> ToBuffer()
        {
            return (IBuffer<T>) GCHandle.FromIntPtr(_buffer).Target;
        }

        public int capacity => realBuffer.capacity;
        public Allocator allocationStrategy => _nativeAllocator;

        public void Resize(uint newCapacity)
        {
            Check.Require(newCapacity > 0, "Resize requires a size greater than 0");
            Check.Require(newCapacity > capacity, "can't resize to a smaller size");

            var pointer = realBuffer.ToNativeArray(out _);
            var sizeOf  = MemoryUtilities.SizeOf<T>();
            pointer = MemoryUtilities.Realloc(pointer, (uint) (capacity * sizeOf), (uint) (newCapacity * sizeOf), 
                                              Allocator.Persistent);
            NB<T>      b      = new NB<T>(pointer, newCapacity);
            IBuffer<T> buffer = b;

            _buffer    = GCHandle.ToIntPtr(GCHandle.Alloc(buffer, GCHandleType.Normal));
            realBuffer = b;
        }

        public void Dispose()
        {
            if (realBuffer.ToNativeArray(out _) != IntPtr.Zero)
            {
                GCHandle.FromIntPtr(_buffer).Free();
                MemoryUtilities.Free(realBuffer.ToNativeArray(out _), Allocator.Persistent);
            }
            else
                Console.LogWarning($"trying to dispose disposed buffer. Type held: {typeof(T)}");
            
            realBuffer = default;
        }
    }
}