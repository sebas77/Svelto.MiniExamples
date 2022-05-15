using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.Common;

namespace Svelto.DataStructures.Native
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, external datastructure can use interchangeably native and managed memory. 
    /// </summary>
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
#if DEBUG && !PROFILE_SVELTO
        static NativeStrategy()
        {
            if (TypeType.isUnmanaged<T>() == false)
                throw new DBC.Common.PreconditionException("Only unmanaged data can be stored natively");
        }
#endif
        public NativeStrategy(uint size, Allocator allocator, bool clear = true) : this()
        {
            Alloc(size, allocator, clear);
        }

        public int       capacity           => _realBuffer.capacity;

        public void Alloc(uint newCapacity, Allocator allocator, bool clear)
        {
#if DEBUG && !PROFILE_SVELTO
            if (!(this._realBuffer.ToNativeArray(out _) == IntPtr.Zero))
                throw new DBC.Common.PreconditionException("can't alloc an already allocated buffer");
#endif
            _nativeAllocator = allocator;

            IntPtr   realBuffer = MemoryUtilities.Alloc<T>(newCapacity, _nativeAllocator, clear);
            NB<T> b          = new NB<T>(realBuffer, newCapacity);
            _invalidHandle = true;
            _realBuffer    = b;
        }

        public void Resize(uint newSize, bool copyContent = true)
        {
            if (newSize != capacity)
            {
                IntPtr pointer = _realBuffer.ToNativeArray(out _);
                pointer = MemoryUtilities.Realloc<T>(pointer, newSize, _nativeAllocator
                                                   , newSize > capacity ? (uint) capacity : newSize
                                                   , copyContent);
                NB<T> b = new NB<T>(pointer, newSize);
                _realBuffer    = b;
                _invalidHandle = true;
            }
        }

        public IntPtr AsBytesPointer()
        {
            throw new NotImplementedException();
        }

        public void   SerialiseFrom(IntPtr bytesPointer)
        {
            throw new NotImplementedException();
        }

        public void ShiftLeft(uint index, uint count)
        {
            DBC.Common.Check.Require(index < capacity, "out of bounds index");
            DBC.Common.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.Common.Check.Require(count > index, "wrong parameters used");

            var array = _realBuffer.ToNativeArray(out _);

            MemoryUtilities.MemMove<T>(array, index + 1, index, count - index);
        }

        public void ShiftRight(uint index, uint count)
        {
            DBC.Common.Check.Require(index < capacity, "out of bounds index");
            DBC.Common.Check.Require(count < capacity, "out of bounds count");

            if (count == index)
                return;

            DBC.Common.Check.Require(count > index, "wrong parameters used");

            var array = _realBuffer.ToNativeArray(out _);

            MemoryUtilities.MemMove<T>(array, index, index + 1, count - index);
        }

        public bool isValid => _realBuffer.isValid;

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
            if (_invalidHandle == true && ((IntPtr) _cachedReference != IntPtr.Zero))
            {
                _cachedReference.Free();
                _cachedReference = default;
            }

            _invalidHandle = false;
            if (((IntPtr) _cachedReference == IntPtr.Zero))
            {
                _cachedReference = GCHandle.Alloc(_realBuffer, GCHandleType.Normal);
            }

            return (IBuffer<T>) _cachedReference.Target;
        }

        public NB<T> ToRealBuffer()
        {
            return _realBuffer;
        }

        public void Dispose()
        {
            ReleaseCachedReference();

            if (_realBuffer.ToNativeArray(out _) != IntPtr.Zero)
                MemoryUtilities.Free(_realBuffer.ToNativeArray(out _), _nativeAllocator);
            else
                throw new Exception("trying to dispose disposed buffer");

            _cachedReference = default;
            _realBuffer      = default;
        }

#if UNITY_BURST 
        [Unity.Burst.BurstDiscard]
#endif        
        void ReleaseCachedReference()
        {
            if ((IntPtr)_cachedReference != IntPtr.Zero)
                _cachedReference.Free();
        }

        Allocator _nativeAllocator;
        NB<T>      _realBuffer;
        bool       _invalidHandle;

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        GCHandle _cachedReference;
    }
}