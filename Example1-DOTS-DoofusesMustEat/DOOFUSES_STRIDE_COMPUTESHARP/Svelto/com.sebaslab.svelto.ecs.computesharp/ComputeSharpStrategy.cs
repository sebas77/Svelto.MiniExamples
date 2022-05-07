using System;
using System.Runtime.CompilerServices;
using DBC.ECS.Compute;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, external datastructure can use interchangeably native and managed memory. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComputeSharpStrategy<T> : IBufferStrategy<T> where T : unmanaged
    {
        public ComputeSharpStrategy(uint size, bool clear) : this()
        {
            Alloc(size, Allocator.None, clear);
        }

        public bool isValid => _buffer != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(uint size, Allocator allocator, bool clear = true)
        {
            var realBuffer = new ComputeSharpBuffer<T>(size, clear);
            _realBuffer = realBuffer;
            _buffer     = _realBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize, bool copyContent = true)
        {
            if (newSize != capacity)
            {
                var realBuffer = new ComputeSharpBuffer<T>(newSize, true);
                if (copyContent == true)
                     _realBuffer.CopyTo(ref realBuffer);
                
                _realBuffer = realBuffer;
                _buffer     = _realBuffer;
            }
        }

        public IntPtr AsBytesPointer()
        {
            throw new NotImplementedException();
        }

        public void SerialiseFrom(IntPtr bytesPointer)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShiftLeft(uint index, uint count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShiftRight(uint index, uint count)
        {
            throw new NotImplementedException();
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _realBuffer.Clear();
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[(int)index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public ComputeSharpBuffer<T> ToRealBuffer()
        {
            return _realBuffer;
        }

        IBuffer<T> IBufferStrategy<T>.ToBuffer()
        {
            Check.Require(_buffer != null, "Buffer not found in expected state");

            return _buffer;
        }

        public void Dispose()
        {
            _realBuffer.Dispose();
        }

        IBuffer<T>            _buffer;
        ComputeSharpBuffer<T> _realBuffer;
    }
}