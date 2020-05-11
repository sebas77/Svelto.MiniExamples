using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// MB stands for ManagedBuffer
    ///
    /// MB are wrappers of arrays. Are not meant to resize or free
    /// MBs cannot have a count, because a count of the meaningful number of items is not tracked.
    /// Example: an MB could be initialized with a size 10 and count 0. Then the buffer is used to fill entities
    /// but the count will stay zero. It's not the MB responsibility to track the count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct MB<T>:IBuffer<T> 
    {
        public MB(T[]  array) : this()
        {
            _buffer = array;
        }
        
        public void Set(T[] array)
        {
            _buffer = array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint size)
        {
            Array.Copy(_buffer, sourceStartIndex, destination, destinationStartIndex, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_buffer, (int) 0, (int) _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return _buffer;
        }

        public IntPtr ToNativeArray(out int capacity)
        {
            throw new NotImplementedException();
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
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

        T[] _buffer;
    }
}