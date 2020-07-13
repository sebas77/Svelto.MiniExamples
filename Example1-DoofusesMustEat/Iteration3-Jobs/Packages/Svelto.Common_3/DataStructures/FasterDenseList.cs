#if later
using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public class FasterDenseList<T>:IEnumerable<T>
    {
        public uint Count => (uint) values.Count;

        public FasterDenseList()
        {
            values = new FasterListStruct<T>(0);
        }
        
        public FasterSparseList<T> SparseSet()
        {
            return new FasterSparseList<T>(this);
        }
        
        public FasterDenseListEnumerator<T> GetEnumerator()
        {
            return new FasterDenseListEnumerator<T>(this);
        }
        
        public bool TryGetValue(uint index, out T item)
        {
            if (index < Count)
            {
                item = values[index];
                return true;
            }

            item = default;
            return false;
        }

        public void FastClear()
        {
            values.ResetToReuse();
        }
        
        public void Clear()
        {
            values.Clear();
        }
        
        public bool ContainsKey(uint gid)
        {
            return gid < Count;
        }
        
        internal bool ReuseOneSlot<U>(uint index, out U item) where U:class, T
        {
            if (values.ReuseOneSlot(out item) == true)
                return true;

            item = default;
            return false;
        }
        
        internal uint Push((uint index, T item) value)
        {
            return values.Push(value.item);
        }

        public ref T this[uint index] => ref values[index];

        public void Add(uint index, in T item)
        {
            values.Add(index, item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        readonly FasterListStruct<T>    values;
    }

    public struct FasterDenseListEnumerator<T>:IEnumerator<T>
    {
        internal FasterDenseListEnumerator(FasterDenseList<T> denseList):this()
        {
            _fasterDenseList = denseList;
            _currentIndex = -1;
        }

        public bool MoveNext()
        {
            if (++_currentIndex < _fasterDenseList.Count)
                return true;

            return false;
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public ref T Current => ref _fasterDenseList[(uint) _currentIndex];

        object IEnumerator.Current => throw new NotImplementedException();
        T IEnumerator<T>.Current => throw new NotImplementedException();

        public void Dispose()
        {}
        
        readonly FasterDenseList<T> _fasterDenseList;
        int                         _currentIndex;
    }
}
#endif