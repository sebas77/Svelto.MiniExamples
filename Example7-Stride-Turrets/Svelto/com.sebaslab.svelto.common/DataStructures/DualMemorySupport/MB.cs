using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// MB stands for ManagedBuffer
    ///
    /// MBs are note meant to be resized or freed. They are wrappers of constant size arrays.
    /// MBs always wrap external arrays, they are not meant to allocate memory by themselves.
    ///
    /// MB are wrappers of arrays. Are not meant to resize or free
    /// MBs cannot have a count, because a count of the meaningful number of items is not tracked.
    /// Example: an MB could be initialized with a size 10 and count 0. Then the buffer is used to fill entities
    /// but the count will stay zero. It's not the MB responsibility to track the count
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct MB<T>:IBuffer<T> 
    {
        internal MB(T[]  array) : this()
        {
            _buffer = array;
        }
        
        public void Set(T[] array)
        {
            _buffer = array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] collection, uint actualSize)
        {
            Array.Copy(collection, 0, _buffer, 0, actualSize);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            Array.Copy(_buffer, sourceStartIndex, destination, destinationStartIndex, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_buffer, (int) 0, (int) _buffer.Length);
        }
        
        public void FastClear() { }

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
        public bool isValid => _buffer != null;
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS                
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif
                
                return ref _buffer[index];
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && ENABLE_PARANOID_CHECKS                
                if (index >= _buffer.Length)
                    throw new IndexOutOfRangeException("Paranoid check failed!");
#endif

                return ref _buffer[index];
            }
        }

        T[]         _buffer;
    }
}