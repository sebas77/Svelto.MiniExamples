#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures.Experimental
{
    public struct FlaggedItem<T>
    {
        public T Item;
        public uint NextUnusedIndex; // -1 indicates the cell is used, otherwise points to the next unused cell
    }

    /// <summary>
    /// DO NOT USE, STILL WORKING ON IT, DOESN't WORK
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TombstoneList<T>
    {
        public TombstoneList()
        {
            _count = 0;
            _firstUnusedIndex = 1;

            _buffer = Array.Empty<FlaggedItem<T>>();
        }

        public TombstoneList(uint initialSize)
        {
            _count = 0;
            _firstUnusedIndex = 1;

            _buffer = new FlaggedItem<T>[initialSize];
        }

        public TombstoneList(int initialSize) : this((uint)initialSize)
        {
        }

        public int count    => (int)_count;
        public int capacity => _buffer.Length;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (index >= _largestUsedIndex)
                    throw new Exception($"TombstoneList - out of bound access: index {index} - count {_largestUsedIndex}");
#endif                
                return ref _buffer[(uint)index].Item;
            }
        }

        public ref T this[uint index]
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
        {
#if ENABLE_DEBUG_CHECKS
                if (index >= _largestUsedIndex)
                    throw new Exception($"TombstoneList - out of bound access: index {index} - count {_largestUsedIndex}");
#endif                
                return ref _buffer[index].Item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(in T item)
        {
            AllocateMore();

            var indexToUse = _firstUnusedIndex - 1;
            _buffer[indexToUse].Item = item;
            _count++;
            
            var nextUnusedIndex = _buffer[indexToUse].NextUnusedIndex; //was the cell pointing to another unused cell?
            if (nextUnusedIndex > 0) //yes
                _firstUnusedIndex = nextUnusedIndex;
            else //if this happens, the linked list is exhausted, all the empty slots are used and so nextUnusedIndex is the actual count (+1 because it's in base 1)
                _firstUnusedIndex = _count; //no
            
            _buffer[indexToUse].NextUnusedIndex = 0; //mark the cell as used

            if (_count > _largestUsedIndex)
                _largestUsedIndex = _count;

            return indexToUse;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index)
        {
            DBC.Common.Check.Require(index                          < _count, "out of bound index");
            DBC.Common.Check.Require(index                          >= 0, "out of bound index");
            DBC.Common.Check.Require(_buffer[index].NextUnusedIndex == 0, "trying to access a tombstone");
            
            _buffer[index].NextUnusedIndex = _firstUnusedIndex;
            _firstUnusedIndex = index + 1; //updating linked list and first empty slot in base 1
            _count--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TombstoneListEnumerator<T> GetEnumerator()
        {
            return new TombstoneListEnumerator<T>(this, _largestUsedIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AllocateMore()
        {
            if (_count == _buffer.Length)
            {
                var newLength = (int)((_buffer.Length + 1) * 1.5f);
                FlaggedItem<T>[] newList   = new FlaggedItem<T>[newLength];
                Array.Copy(_buffer, newList, _count);
                _buffer = newList;
            }
        }

        internal FlaggedItem<T>[]  _buffer;
        uint _firstUnusedIndex;
        uint _count;
        uint _largestUsedIndex;
    }

    public ref struct TombstoneListEnumerator <T>
    {
        public TombstoneListEnumerator(TombstoneList<T> buffer, uint size)
        {
            _counter = 0;
            _buffer = buffer;
            _size = size;
        }
        
        public ref T Current
        {
            get
            {
                DBC.Common.Check.Require(_counter <= _size);
                return ref _buffer._buffer[(uint)_counter - 1].Item;
            }
        }
        
        public uint CurrentIndex => (uint)_counter - 1;

        public bool MoveNext()
        {
            while (_buffer._buffer[_counter++].NextUnusedIndex != 0)
            {
                if (_counter > _size)
                    return false;
            }
            
            return true;
        }

        public void Reset()
        {
            _counter = 0;
        }

        readonly TombstoneList<T>  _buffer;
        int           _counter;
        readonly uint _size;
    }
}