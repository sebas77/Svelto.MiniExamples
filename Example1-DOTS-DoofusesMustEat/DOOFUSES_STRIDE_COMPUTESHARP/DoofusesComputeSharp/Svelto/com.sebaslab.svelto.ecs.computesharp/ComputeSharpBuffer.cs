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

        readonly ReadWriteBuffer<T> _buffer;

        public ComputeSharpBuffer(uint newSize, bool clear) : this()
        {
            _buffer = GraphicsDevice.Default.AllocateReadWriteBuffer<T>((int)newSize,
                clear ? AllocationMode.Clear : AllocationMode.Default);
        }

        public ref T this[int index] => ref _buffer[index];
    }
}