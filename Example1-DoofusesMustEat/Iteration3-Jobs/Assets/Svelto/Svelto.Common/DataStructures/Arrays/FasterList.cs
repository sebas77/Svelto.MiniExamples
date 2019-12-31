using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public class FasterList<T>
    {
        internal static FasterList<T> DefaultEmptyList = new FasterList<T>();
        
        public int Count => (int) _count;
        public uint Capacity => (uint) _buffer.Length;

        public bool IsReadOnly => false;
        
        public FasterList()
        {
            _count = 0;

            _buffer = new T[0];
        }

        public FasterList(uint initialSize)
        {
            _count = 0;

            _buffer = new T[initialSize];
        }

        public FasterList(int initialSize)
        {
            _count = 0;

            _buffer = new T[initialSize];
        }

        public FasterList(T[] collection)
        {
            _buffer = new T[collection.Length];

            Array.Copy(collection, _buffer, collection.Length);

            _count = (uint) collection.Length;
        }

        public FasterList(T[] collection, uint actualSize)
        {
            _buffer = new T[actualSize];
            Array.Copy(collection, _buffer, actualSize);

            _count = actualSize;
        }

        public FasterList(ICollection<T> collection)
        {
            _buffer = new T[collection.Count];

            collection.CopyTo(_buffer, 0);

            _count = (uint) collection.Count;
        }

        public FasterList(ICollection<T> collection, int extraSize)
        {
            _buffer = new T[(uint) collection.Count + (uint)extraSize];

            collection.CopyTo(_buffer, 0);
            
            _count = (uint) collection.Count;
        }
        
        public FasterList(in FasterList<T> source)
        {
            _buffer = new T[ source.Count];

            source.CopyTo(_buffer, 0);

            _count = (uint) source.Count;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                DBC.Common.Check.Require(index < _count && _count > 0, "out of bound index");
                return ref _buffer[(uint) index];
            }
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                DBC.Common.Check.Require(index < _count, $"out of bound index '{index}'. Size of list is '{_count}'");
                return ref _buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            if (_count == _buffer.Length)
                AllocateMore();

            _buffer[_count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint location, in T item)
        {
            Expand(location + 1);

            _buffer[location] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(in FasterList<T> items)
        {
            AddRange(items._buffer, (uint) items.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] items, uint count)
        {
            if (count == 0) return;

            if (_count + count > _buffer.Length)
                AllocateMore(_count + count);

            Array.Copy(items, 0, _buffer, _count, count);
            _count += count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] items)
        {
            AddRange(items, (uint) items.Length);
        }

        /// <summary>
        /// Careful, you could keep on holding references you don't want to hold to anymore
        /// Use Clear in case.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
#if DEBUG && !PROFILER
            if (typeof(T).IsClass)
                Console.LogWarning(
                    "Warning: objects held by this list won't be garbage collected. Use ResetToReuse or Clear " +
                    "to avoid this warning");
#endif
            _count = 0;
        }
        
        /// <summary>
        /// this is a dirtish trick to be able to use the index operator
        /// before adding the elements through the Add functions
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="initialSize"></param>
        /// <returns></returns>
        public static FasterList<T> PreFill<U>(uint initialSize) where U: T, new()
        {
            var list = new FasterList<T>(initialSize);

             if (default(U) == null)
            {
                for (int i = 0; i < initialSize; i++)
                    list._buffer[(uint) (i)] = new U();
            }

            return list;
        }

        public static FasterList<T> Fill<U>(uint initialSize) where U: T, new()
        {
            var list = PreFill<U>(initialSize);

            list._count = initialSize;

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetToReuse()
        {
            _count = 0;
        }

        public bool ReuseOneSlot<U>(out U result) where U:T 
        {
            if (_count >= _buffer.Length)
            {
                result = default(U);
                
                return false;
            }

            if (default(U) == null)
            {
                result = (U) _buffer[_count];

                if (result != null)
                {
                    _count++;
                    return true;
                }
                return false;
            }

            _count++;
            result = default(U);
            return true;
        }
        
        public bool ReuseOneSlot<U>() where U: T
        {
            if (_count >= _buffer.Length)
                return false;

            _count++;

            return true;
        }
        
        public bool ReuseOneSlot()
        {
            if (_count >= _buffer.Length)
                return false;

            _count++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);

            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            var index = IndexOf(item);

            return index != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator<T> GetEnumerator()
        {
            return new FasterListEnumerator<T>(_buffer, (uint) Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
        {
            var comp = EqualityComparer<T>.Default;

            for (uint index = 0; index < _count; index++)
                if (comp.Equals(_buffer[index], item))
                    return (int) index;

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            DBC.Common.Check.Require(index < _count, "out of bound index");

            if (_count == _buffer.Length) AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);
            ++_count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            RemoveAt(index);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            DBC.Common.Check.Require(index < _count, "out of bound index");

            if (index == --_count)
                return;

            Array.Copy(_buffer, index + 1, _buffer, index, _count - index);

            _buffer[_count] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize)
        {
            if (newSize == _buffer.Length) return;
            
            Array.Resize(ref _buffer, (int) newSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            var destinationArray = new T[_count];

            Array.Copy(_buffer, 0, destinationArray, 0, _count);

            return destinationArray;
        }

        /// <summary>
        /// This function exists to allow fast iterations. The size of the array returned cannot be
        /// used. The list count must be used instead.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArrayFast(out uint count)
        {
            count = _count;

            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArrayFast()
        {
            return _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UnorderedRemove(T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            UnorderedRemoveAt(index);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UnorderedRemoveAt(int index)
        {
            DBC.Common.Check.Require(index < _count && _count > 0, "out of bound index");

            if (index == --_count)
            {
                _buffer[_count] = default;
                return false;
            }

            _buffer[(uint) index] = _buffer[_count];
            _buffer[_count] = default;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AllocateMore()
        {
            var newList = new T[(_buffer.Length + 1) << 1];
            if (_count > 0) Array.Copy(_buffer, newList, _count);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AllocateMore(uint newSize)
        {
            DBC.Common.Check.Require(newSize > _buffer.Length);
            var newLength = Math.Max(_buffer.Length, 1);

            while (newLength < newSize)
                newLength <<= 1;

            var newList = new T[newLength];
            if (_count > 0) Array.Copy(_buffer, newList, _count);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_count < _buffer.Length)
                Resize(_count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimCount(uint newCount)
        {
            DBC.Common.Check.Require(_count >= newCount, "the new length must be less than the current one");

            _count = newCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExpandBy(uint increment)
        {
            uint count = _count + increment;

            if (_buffer.Length < count)
                AllocateMore(count);

            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(uint newSize)
        {
            if (_buffer.Length < newSize)
                AllocateMore(newSize);

            if (_count < newSize)
                _count = newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Enqueue(in T item)
        {
            Add(_count, item);

            return _count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Dequeue() { 
            --_count;
            return ref _buffer[_count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Peek()
        {
            return ref _buffer[_count - 1];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_buffer, 0, array, arrayIndex, Count);
        }

        public void Dispose()
        { }

        T[] _buffer;
        uint    _count;

        public static class NoVirt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Count(FasterList<T> fasterList)
            {
                return (uint) fasterList._count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T[] ToArrayFast(FasterList<T> fasterList, out uint count)
            {
                count = (uint) fasterList._count;

                return fasterList._buffer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T[] ToArrayFast(FasterList<T> fasterList)
            {
                return fasterList._buffer;
            }
        }
    }
}