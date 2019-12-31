using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    public struct ManagedBuffer<T>:IBuffer<T> 
    {
        public void Set(T[] array)
        {
            _buffer = array;
        }

        public void Set<Buffer1>(Buffer1 array) where Buffer1 : IBuffer<T>
        {
            _buffer = array.ToManagedArray();
        }

        public void Set(GCHandle handle, uint count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint initialSize)
        {
            _buffer = new T[initialSize];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom<TBuffer>(TBuffer array, uint startIndex, uint size) where TBuffer:IBuffer<T>
        {
            array.CopyTo(_buffer, 0, startIndex, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] source, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            Array.Copy(source, sourceStartIndex, _buffer, destinationStartIndex, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] destination, uint sourceStartIndex, uint destinationStartIndex, uint size)
        {
            Array.Copy(_buffer, sourceStartIndex, destination, destinationStartIndex, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(ICollection<T> source)
        {
            source.CopyTo(_buffer, source.Count); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint startIndex, uint count)
        {
            Array.Clear(_buffer, (int) startIndex, (int) count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_buffer, (int) 0, (int) _buffer.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize)
        {
            Array.Resize(ref _buffer, (int) newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item, int count)
        {
            Array.Copy(_buffer, index, _buffer, index + 1, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, int count)
        {
            Array.Copy(_buffer, index + 1, _buffer, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return _buffer;
        }

        public IntPtr ToNativeArray()
        {
            throw new NotImplementedException();
        }

        public GCHandle Pin()
        {
            return GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            _buffer = null;
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }
        
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        public uint length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint) _buffer.Length;
        }

        T[] _buffer;
    }
}