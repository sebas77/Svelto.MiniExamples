using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Svelto.ECS")]

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
    internal struct MBInternal<T>:IBuffer<T> 
    {
        MBInternal(T[]  array) : this()
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
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return _buffer;
        }
        
        public static implicit operator MB<T>(MBInternal<T> proxy) => new MB<T>(proxy);
        public static implicit operator MBInternal<T>(MB<T> proxy) => new MBInternal<T>(proxy.ToManagedArray());
        
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

    public ref struct MB<T>
    {
        MBInternal<T> _bufferImplementation;

        internal MB(MBInternal<T> mbInternal)
        {
            _bufferImplementation = mbInternal;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            _bufferImplementation.CopyTo(sourceStartIndex, destination, destinationStartIndex, count);
        }

        public void Clear()
        {
            _bufferImplementation.Clear();
        }

        public int capacity => _bufferImplementation.capacity;

        public bool isValid => _bufferImplementation.isValid;
        
        public void Set(T[] array)
        {
            _bufferImplementation.Set(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(T[] collection, uint actualSize)
        {
            _bufferImplementation.CopyFrom(collection, actualSize);
        }
        
        /// <summary>
        /// todo: this must go away, it's not safe. it must become internal and only used by the framework
        /// externally should use the AsReader, AsWriter, AsReadOnly, AsParallelReader, AsParallelWriter pattern
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            return  _bufferImplementation.ToManagedArray();
        }
        
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _bufferImplementation[index];
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

                return ref _bufferImplementation[index];
            }
        }
    }
}