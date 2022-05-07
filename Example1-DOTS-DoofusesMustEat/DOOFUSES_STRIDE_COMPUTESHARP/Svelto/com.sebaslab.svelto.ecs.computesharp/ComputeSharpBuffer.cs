using System;
using ComputeSharp;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public readonly struct ComputeSharpBuffer<T> : IBuffer<T> where T : unmanaged
    {
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public T[] ToManagedArray()
        {
            throw new NotImplementedException();
        }

        public IntPtr ToNativeArray(out int capacity)
        {
            throw new NotImplementedException();
        }

        public int  capacity { get; }
        public bool isValid  { get; }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        public ComputeSharpBuffer(uint newSize, bool clear) : this()
        {
            _buffer = GraphicsDevice.Default.AllocateReadBackBuffer<T>((int)newSize
                                                                   , clear
                                                                         ? AllocationMode.Clear
                                                                         : AllocationMode.Default);
        }
        
        public void CopyTo(ref ComputeSharpBuffer<T> destination)
        {
            Span<T> bufferSpan = destination._buffer.Span;
            
            _buffer.Span.CopyTo(bufferSpan);
        }

        public ref T this[int index] => ref _buffer.Span[index];
        public ref T this[uint index] => ref this[(int)index];

        readonly ReadBackBuffer<T>  _buffer;
        readonly ReadWriteBuffer<T> _gpuBuffer;
    }
}