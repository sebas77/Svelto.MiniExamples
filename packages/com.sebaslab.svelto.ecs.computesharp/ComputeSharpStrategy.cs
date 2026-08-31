using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ComputeSharp;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, external datastructure can use interchangeably native and managed memory. 
    /// </summary>
    public struct ComputeSharpStrategy<T>: IBufferStrategy<T>
            where T : unmanaged
    {
        public ComputeSharpStrategy(uint size, Allocator allocator, bool clear = true): this()
        {
            Alloc(size, allocator, clear);
        }

        public int capacity => _realBuffer.capacity;

        public void Alloc(uint newCapacity, Allocator allocator, bool clear)
        {
            _graphicsDevice = _cachedGraphicsDevice;
            
#if DEBUG && !PROFILE_SVELTO
            if ((this._realBuffer.isValid))
                throw new DBC.ECS.Compute.PreconditionException("can't alloc an already allocated buffer");
#endif
            UploadBuffer<T> realBuffer = _graphicsDevice.AllocateUploadBuffer<T>(
                (int)newCapacity, clear ? AllocationMode.Clear : AllocationMode.Default);
            ReadWriteBuffer<T> readWriteBuffer = _graphicsDevice.AllocateReadWriteBuffer<T>((int)newCapacity);
            ComputeSharpBuffer<T> b = new ComputeSharpBuffer<T>(realBuffer, readWriteBuffer);
            _realBuffer = b;
        }

        public void Resize(uint newSize, bool copyContent = true, bool memClear = true)
        {
            if (newSize == capacity)
                return;

            var allocationMode = memClear ? AllocationMode.Clear : AllocationMode.Default;
            var uploadBuffer = _graphicsDevice.AllocateUploadBuffer<T>((int)newSize, allocationMode);
            var readWriteBuffer = _graphicsDevice.AllocateReadWriteBuffer<T>((int)newSize, allocationMode);
            var resizedBuffer = new ComputeSharpBuffer<T>(uploadBuffer, readWriteBuffer);

            if (copyContent == true && _realBuffer.isValid == true)
            {
                var oldCapacity = capacity;
                var copyCount = oldCapacity < (int)newSize ? oldCapacity : (int)newSize;

                for (var i = 0; i < copyCount; i++)
                    resizedBuffer[i] = _realBuffer[i];

                resizedBuffer.ToComputeBuffer();
            }

            if (_realBuffer.isValid == true)
                _realBuffer.Dispose();

            _realBuffer = resizedBuffer;
        }

        public IntPtr AsBytesPointer()
        {
            throw new NotSupportedException(
                "ComputeSharpStrategy does not expose raw byte pointers. This path is disabled to avoid implicit GPU readbacks.");
        }

        public void SerialiseFrom(IntPtr bytesPointer)
        {
            throw new NotSupportedException(
                "ComputeSharpStrategy cannot deserialize from raw pointers without forcing expensive staging copies.");
        }

        public void ShiftLeft(uint index, uint count)
        {
            DBC.ECS.Compute.Check.Require(index < capacity, "out of bounds index");
            DBC.ECS.Compute.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.ECS.Compute.Check.Require(count > index, "wrong parameters used");

            for (var i = index; i < count; i++)
                _realBuffer[i] = _realBuffer[(int)i + 1];

            _realBuffer.ToComputeBuffer();
        }

        public void ShiftRight(uint index, uint count)
        {
            DBC.ECS.Compute.Check.Require(index < capacity, "out of bounds index");
            DBC.ECS.Compute.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.ECS.Compute.Check.Require(count > index, "wrong parameters used");

            for (var i = (int)count; i > (int)index; i--)
                _realBuffer[i] = _realBuffer[i - 1];

            _realBuffer.ToComputeBuffer();
        }

        public bool isValid => _realBuffer.isValid;

        public void Clear() => _realBuffer.Clear();
        public void FastClear() { }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }
        
        [ThreadStatic]
        private static IBuffer<T> s_box;

        /// <summary>
        /// Note on the code of this method. Interfaces cannot be held by this structure as it must be used by Burst.
        /// This method could return directly _realBuffer, but this would cost of a boxing allocation.
        /// Using the GCHandle.Alloc I will occur to the boxing, but only once as long as the native handle is still
        /// valid
        /// </summary>
        /// <returns></returns>
        IBuffer<T> IBufferStrategy<T>.ToBuffer()
        {
            IBuffer<T> box = s_box;

            if (box == null)
            {
                // ThreadStatic box not been created yet.
                s_box = box = _realBuffer;
            }
            
            ref ComputeSharpBuffer<T> unboxed = ref Unsafe.Unbox<ComputeSharpBuffer<T>>(box);
            // Copy to boxed ref so everything else is same
            unboxed = _realBuffer;
            
            return box;
        }

        public ComputeSharpBuffer<T> ToRealBuffer()
        {
            return _realBuffer;
        }

        public void Dispose()
        { 
            _realBuffer.Dispose();

            _realBuffer = default;
        }
        
        ComputeSharpBuffer<T> _realBuffer;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        GraphicsDevice _graphicsDevice;

        static readonly GraphicsDevice _cachedGraphicsDevice = GraphicsDevice.EnumerateDevices().First();
    }
}
