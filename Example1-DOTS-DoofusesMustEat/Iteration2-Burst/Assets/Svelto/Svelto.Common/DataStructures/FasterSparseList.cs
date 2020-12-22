using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public struct FasterSparseList<T> : IEnumerable<T>
    {
        internal FasterSparseList(FasterDenseList<T> denseSet)
        {
            _denseSet = denseSet;
            _keys = new FasterList<uint>();
        }

        public int Count => _keys.Count;
        
        internal ref T this[uint index] => ref _denseSet[_keys[index] - 1];

        public bool TryGetValue(uint key, out T item)
        {
            if (key < _keys.Count && _keys[key] != NOT_USED_SLOT && _keys[key] - 1 < _denseSet.Count)
            {
                item = _denseSet[_keys[key] - 1];
                return true;
            }

            item = default;
            return false;
        }
        
        public bool TryRecycleValue<U>(uint key, out T item) where U:class, T
        {
            if (TryGetValue(key, out item) == true) return true;

            if (_denseSet.ReuseOneSlot<U>(key, out var item2) == true)
            {
                item = item2;
                _keys.Add(key, _denseSet.Count);

                return true;
            }

            return false;
        }

        public void Add(uint key, T  item)
        {
            var index = _denseSet.Push((key, item));
            
            _keys.Add(key, index);
        }
        
        public void FastClear()
        {
            _keys.FastClear();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        public FasterSparseListEnumerator<T> GetEnumerator()
        {
            return new FasterSparseListEnumerator<T>(this);
        }
        
        readonly FasterDenseList<T> _denseSet;
        readonly FasterList<uint>   _keys;

        const ulong NOT_USED_SLOT = 0;
        
        public class Iterator
        {
            public Iterator(uint currentIndex, in FasterSparseList<T> fasterSparseList)
            {
                _fasterSparseList = fasterSparseList;
                _currentIndex = currentIndex;
            }
            
            public ref T Value => ref _fasterSparseList[_fasterSparseList._keys[_currentIndex]];
            public uint Key => _fasterSparseList._keys[_currentIndex];
            
            FasterSparseList<T> _fasterSparseList;
            readonly uint _currentIndex;
        }
    }
    
    public struct FasterSparseListEnumerator<T>:IEnumerator<FasterSparseList<T>.Iterator>
    {
        internal FasterSparseListEnumerator(FasterSparseList<T> sparseList):this()
        {
            _fasterSparseList = sparseList;
            _currentIndex = -1;
        }

        public bool MoveNext()
        {
            if (++_currentIndex < _fasterSparseList.Count)
                return true;

            return false;
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public FasterSparseList<T>.Iterator Current =>
            new FasterSparseList<T>.Iterator((uint) _currentIndex, _fasterSparseList);

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {}
        
        readonly FasterSparseList<T> _fasterSparseList;
        int                         _currentIndex;
    }
}