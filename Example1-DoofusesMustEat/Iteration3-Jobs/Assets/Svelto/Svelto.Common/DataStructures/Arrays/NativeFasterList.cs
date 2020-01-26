using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct EmptyStruct
    {
    }
    
    public struct NativeFasterList<T>:IDisposable where T : unmanaged
    {
        public uint count => _count;
        public uint capacity { get; private set; }

        public NativeFasterList(uint initialSize)
        {
            unsafe
            {
                _count = 0;
                capacity = initialSize;

                _buffer = (T*)Marshal.AllocHGlobal((int) (sizeof(T) * initialSize));
            }
        }

        public NativeFasterList(int initialSize):this((uint)initialSize)
        { }
        
        public void Dispose()
        {
            unsafe
            {
                Marshal.FreeHGlobal((IntPtr) _buffer);
                _count = 0;
                capacity = 0;
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    DBC.Common.Check.Require(index < _count && _count > 0, "out of bound index");
                    return ref _buffer[(uint) index];
                }
            }
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    DBC.Common.Check.Require(index < _count, "out of bound index");
                    return ref _buffer[index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            unsafe
            {
                if (_count == capacity)
                    AllocateMore();

                _buffer[_count++] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint location, in T item)
        {
            unsafe
            {
                ExpandTo(location + 1);

                _buffer[location] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(in NativeFasterList<T> items)
        {
            unsafe
            {
                AddRange(items._buffer, items.count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRange(T* items, uint count)
        {
            if (count == 0) return;

            if (_count + count > capacity)
                AllocateMore(_count + count);

            Copy(items, _buffer, count, _count, capacity);
            
            _count += count;
        }

        /// <summary>
        /// Careful, you could keep on holding references you don't want to hold to anymore
        /// Use Clear in case.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            _count = 0;
        }
        
        public static NativeFasterList<T> Fill(uint initialSize)
        {
            var list = new NativeFasterList<T>(initialSize);

            list._count = initialSize;

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
                var structs = (EmptyStruct*) _buffer;
                var bytes = (byte*) _buffer;

                int counter;
                for (counter = 0; counter < _count >> 4; ++counter) 
                    structs[counter << 4] = new EmptyStruct();
            
                for (int j = counter << 4; j < _count; ++j)
                    bytes[j] = 0;

                _count = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize)
        {
            unsafe
            {
                if (newSize == capacity) return;

                Marshal.ReAllocHGlobal((IntPtr) _buffer, new IntPtr(newSize));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UnorderedRemoveAt(int index)
        {
            unsafe
            {
                DBC.Common.Check.Require(index < _count && _count > 0, "out of bound index");

                if (index == --_count)
                {
                    unsafe
                    {
                        _buffer[_count] = default;
                        return false;
                    }
                }

                _buffer[(uint) index] = _buffer[_count];
                _buffer[_count] = default;

                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_count < capacity)
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

            if (capacity < count)
                AllocateMore(count);

            _count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExpandTo(uint newSize)
        {
            if (capacity < newSize)
                AllocateMore(newSize);

            if (_count < newSize)
                _count = newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Push(in T item)
        {
            Add(_count, item);

            return _count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Pop() {
            unsafe
            {
                --_count;
                return ref _buffer[_count];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Peek()
        {
            unsafe
            {
                return ref _buffer[_count - 1];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AllocateMore()
        {
            var newCapacity = ((capacity + 1) << 1);
            T* newList = (T*) Marshal.AllocHGlobal((int) newCapacity * sizeof(T));
            if (_count > 0) Copy(_buffer, newList, _count, 0, newCapacity);
            Marshal.FreeHGlobal((IntPtr) _buffer);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void AllocateMore(uint newSize)
        {
            DBC.Common.Check.Require(newSize > capacity);
            var newCapacity = Math.Max(capacity, 1);

            while (newCapacity < newSize)
                newCapacity <<= 1;

            T* newList = (T*) Marshal.AllocHGlobal((int) newCapacity * sizeof(T));
            if (_count > 0) Copy(_buffer, newList, _count, 0, newCapacity);
            Marshal.FreeHGlobal((IntPtr) _buffer);
            _buffer = newList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void Copy(T* source, T* dest, uint sourceCount, uint destCount, uint destCapacity)
        {
            Buffer.MemoryCopy(source, //source 
                dest + destCount, //destination
                (destCapacity - destCount) * sizeof(T), // destination size in byte
                sourceCount * sizeof(T)); //number of bytes to copy
        }

        unsafe T* _buffer;
        uint    _count;
    }
}