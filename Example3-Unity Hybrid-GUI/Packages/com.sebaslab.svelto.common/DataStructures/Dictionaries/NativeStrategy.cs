using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DBC.Common;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
#if DEBUG && !PROFILE_SVELTO            
        static NativeStrategy()
        {
            if (TypeCache<T>.IsUnmanaged == false)
                throw new PreconditionException("Only unmanaged data can be stored natively");
        }
#endif        

        public void Alloc(uint newCapacity, Allocator nativeAllocator)
        {
#if DEBUG && !PROFILE_SVELTO            
            if (!(this._realBuffer.ToNativeArray(out _) == IntPtr.Zero))
                throw new PreconditionException("can't alloc an already allocated buffer");
#endif            
            _nativeAllocator = nativeAllocator;

            var realBuffer =
                MemoryUtilities.Alloc((uint) (newCapacity * MemoryUtilities.SizeOf<T>()), _nativeAllocator);
            NB<T> b = new NB<T>(realBuffer, newCapacity);
            _buffer          = default;
            _realBuffer = b;
        }

        public bool isValid => _realBuffer.isValid;

        public NativeStrategy(uint size, Allocator nativeAllocator) : this()
        {
            Alloc(size, nativeAllocator);
        }

        public void Resize(uint newCapacity, bool copyContent = true)
        {
#if DEBUG && !PROFILE_SVELTO            
            if (!(newCapacity > 0))
                throw new PreconditionException("Resize requires a size greater or equal to 0");
            if (!(newCapacity > capacity))
                throw new PreconditionException("can't resize to a smaller size");
#endif            
            var pointer = _realBuffer.ToNativeArray(out _);
            var sizeOf  = MemoryUtilities.SizeOf<T>();
            pointer = MemoryUtilities.Realloc(pointer, (uint) (capacity * sizeOf), (uint) (newCapacity * sizeOf)
                                            , _nativeAllocator, copyContent);
            NB<T> b = new NB<T>(pointer, newCapacity);
            _realBuffer    = b;
            _invalidHandle = true;
        }

        public void Clear()     => _realBuffer.Clear();
        public void FastClear() => _realBuffer.FastClear();

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

        public IBuffer<T> ToBuffer()
        {
            //To use this struct in Burst it cannot hold interfaces. This weird looking code is to
            //be able to store _realBuffer as a c# reference.
            if (_invalidHandle == true && ((IntPtr)_buffer != IntPtr.Zero))
            {
                _buffer.Free();
                _buffer = default;
            }
            _invalidHandle = false;
            if (((IntPtr)_buffer == IntPtr.Zero))
            {
                _buffer = GCHandle.Alloc(_realBuffer, GCHandleType.Normal);
            }

            return (IBuffer<T>) _buffer.Target;
        }

        public NB<T> ToRealBuffer() { return _realBuffer; }

        public int       capacity           => _realBuffer.capacity;
        public Allocator allocationStrategy => _nativeAllocator;

        public void Dispose()
        {
            if ((IntPtr)_buffer != IntPtr.Zero)
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
#if UNITY_NATIVE
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        GCHandle _buffer;
        bool _invalidHandle;
    }
}