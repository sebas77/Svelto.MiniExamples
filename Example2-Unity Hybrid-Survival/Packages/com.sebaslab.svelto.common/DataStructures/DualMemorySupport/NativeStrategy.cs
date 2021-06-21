using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
#if DEBUG && !PROFILE_SVELTO
        static NativeStrategy()
        {
            if (TypeCache<T>.IsUnmanaged == false)
                throw new DBC.Common.PreconditionException("Only unmanaged data can be stored natively");
        }
#endif
        public NativeStrategy(uint size, Allocator allocator, bool clear = true) : this() { Alloc(size, allocator, clear); }

        public int       capacity           => _realBuffer.capacity;
        public Allocator allocationStrategy => _nativeAllocator;

        public void Alloc(uint newCapacity, Allocator allocator, bool clear = true)
        {
#if DEBUG && !PROFILE_SVELTO
            if (!(this._realBuffer.ToNativeArray(out _) == IntPtr.Zero))
                throw new DBC.Common.PreconditionException("can't alloc an already allocated buffer");
#endif
            _nativeAllocator = allocator;

            var   realBuffer = MemoryUtilities.Alloc<T>(newCapacity, _nativeAllocator, clear);
            NB<T> b          = new NB<T>(realBuffer, newCapacity);
            _buffer     = default;
            _realBuffer = b;
        }

        public void AllocateMore(uint newSize, uint numberOfItemsToCopy, bool clear = true)
        {
#if DEBUG && !PROFILE_SVELTO
            if (newSize <= capacity)
                throw new DBC.Common.PreconditionException("can't alloc more to a smaller or equal size");
            if (numberOfItemsToCopy > capacity)
                throw new DBC.Common.PreconditionException("out of bounds number of elements to copy");
#endif
            var pointer = _realBuffer.ToNativeArray(out _);
            pointer = MemoryUtilities.Realloc<T>(pointer, newSize, _nativeAllocator, numberOfItemsToCopy, true, clear);
            NB<T> b = new NB<T>(pointer, newSize);
            _realBuffer    = b;
            _invalidHandle = true;
        }

        public void ShiftLeft(uint index, uint count)
        {
            DBC.Common.Check.Require(index < capacity, "out of bounds index");
            DBC.Common.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.Common.Check.Require(count > index, "wrong parameters used");

            var array = _realBuffer.ToNativeArray(out _);

            MemoryUtilities.Memmove<T>(array, index + 1, index, count - index);
        }

        public void ShiftRight(uint index, uint count)
        {
            DBC.Common.Check.Require(index < capacity, "out of bounds index");
            DBC.Common.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.Common.Check.Require(count > index, "wrong parameters used");

            var array = _realBuffer.ToNativeArray(out _);

            MemoryUtilities.Memmove<T>(array, index, index + 1, count - index);
        }

        public bool isValid => _realBuffer.isValid;

        public void Resize(uint newCapacity, bool copyContent = true)
        {
            var pointer = _realBuffer.ToNativeArray(out _);
            pointer = MemoryUtilities.Realloc<T>(pointer, newCapacity, _nativeAllocator, (uint) capacity, copyContent);
            NB<T> b = new NB<T>(pointer, newCapacity);
            _realBuffer    = b;
            _invalidHandle = true;
        }

        public void Clear() => _realBuffer.Clear();

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

        IBuffer<T> IBufferStrategy<T>.ToBuffer()
        {
            //To use this struct in Burst it cannot hold interfaces. This weird looking code is to
            //be able to store _realBuffer as a c# reference.
            if (_invalidHandle == true && ((IntPtr) _buffer != IntPtr.Zero))
            {
                _buffer.Free();
                _buffer = default;
            }

            _invalidHandle = false;
            if (((IntPtr) _buffer == IntPtr.Zero))
            {
                _buffer = GCHandle.Alloc(_realBuffer, GCHandleType.Normal);
            }

            return (IBuffer<T>) _buffer.Target;
        }

        public NB<T> ToRealBuffer()
        {
            DBC.Common.Check.Require(_buffer != null, "Buffer not found in expected state");

            return _realBuffer;
        }

        public void Dispose()
        {
            if ((IntPtr) _buffer != IntPtr.Zero)
                _buffer.Free();

            if (_realBuffer.ToNativeArray(out _) != IntPtr.Zero)
            {
                MemoryUtilities.Free(_realBuffer.ToNativeArray(out _), Allocator.Persistent);
            }
            else
                throw new Exception("trying to dispose disposed buffer");

            _buffer     = default;
            _realBuffer = default;
        }

        Allocator _nativeAllocator;
        NB<T>     _realBuffer;
#if UNITY_COLLECTIONS
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        GCHandle _buffer;
        bool     _invalidHandle;
    }
}