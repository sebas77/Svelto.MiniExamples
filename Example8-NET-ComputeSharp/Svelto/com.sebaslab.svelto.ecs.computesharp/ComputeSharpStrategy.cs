using System;
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
#if DEBUG && !PROFILE_SVELTO
            if ((this._realBuffer.isValid))
                throw new DBC.ECS.Compute.PreconditionException("can't alloc an already allocated buffer");
#endif
            UploadBuffer<T> realBuffer = GraphicsDevice.GetDefault().AllocateUploadBuffer<T>(
                (int)newCapacity, clear ? AllocationMode.Clear : AllocationMode.Default);
            ReadWriteBuffer<T> readWriteBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<T>((int)newCapacity);

            ComputeSharpBuffer<T> b = new ComputeSharpBuffer<T>(realBuffer, readWriteBuffer);
            _invalidHandle = true;
            _realBuffer = b;
        }

        public void Resize(uint newSize, bool copyContent = true, bool memClear = true)
        {
            throw new NotImplementedException();
//            if (newSize != capacity)
//            {
//                IntPtr pointer = _realBuffer.ToNativeArray(out _);
//                pointer = MemoryUtilities.NativeRealloc<T>(pointer, newSize, _nativeAllocator
//                                                   , newSize > capacity ? (uint) capacity : newSize
//                                                   , copyContent, memClear);
//                ComputeSharpBuffer<T> b = new ComputeSharpBuffer<T>(pointer, newSize);
//                _realBuffer    = b;
//                _invalidHandle = true;
//            }
        }

        public IntPtr AsBytesPointer()
        {
            throw new NotImplementedException();
        }

        public void SerialiseFrom(IntPtr bytesPointer)
        {
            throw new NotImplementedException();
        }

        public void ShiftLeft(uint index, uint count)
        {
            throw new NotImplementedException();
        }

        public void ShiftRight(uint index, uint count)
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Note on the code of this method. Interfaces cannot be held by this structure as it must be used by Burst.
        /// This method could return directly _realBuffer, but this would cost of a boxing allocation.
        /// Using the GCHandle.Alloc I will occur to the boxing, but only once as long as the native handle is still
        /// valid
        /// </summary>
        /// <returns></returns>
#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        IBuffer<T> IBufferStrategy<T>.ToBuffer()
        {
            //handle has been invalidated, dispose of the hold GCHandle (if exists)
            if (_invalidHandle == true && ((IntPtr)_cachedReference != IntPtr.Zero))
            {
                _cachedReference.Free();
                _cachedReference = default;
            }

            _invalidHandle = false;
            if (((IntPtr)_cachedReference == IntPtr.Zero))
            {
                _cachedReference = GCHandle.Alloc(_realBuffer, GCHandleType.Normal);
            }

            return (IBuffer<T>)_cachedReference.Target;
        }

        public ComputeSharpBuffer<T> ToRealBuffer()
        {
            return _realBuffer;
        }

        public void Dispose()
        {
            ReleaseCachedReference();

           _realBuffer.Dispose();

            _cachedReference = default;
            _realBuffer = default;
        }

#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        void ReleaseCachedReference()
        {
            if ((IntPtr)_cachedReference != IntPtr.Zero)
                _cachedReference.Free();
        }

        ComputeSharpBuffer<T> _realBuffer;
        bool _invalidHandle;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        GCHandle _cachedReference;
    }
}