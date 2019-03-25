using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public struct ReadOnlyCollectionStruct<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        public ReadOnlyCollectionStruct(T[] values, uint count)
        {
            _values = values;
            _count = count;
        }

        public ReadOnlyCollectionStructEnumerator<T> GetEnumerator()
        {
            return new ReadOnlyCollectionStructEnumerator<T>(_values, _count);
        }
        
        public T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[i];
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return (int) _count;  }
        }

        public bool IsReadOnly
        {
            get { return true;  }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }
        public object SyncRoot
        {
            get { return null; }
        }

        readonly T[] _values;
        readonly uint _count;
    }
    
    public struct ReadOnlyCollectionStructEnumerator<T>:IEnumerator<T>
    {
        public ReadOnlyCollectionStructEnumerator(T[] values, uint count) : this()
        {
            _index  = 0;
            _values = values;
            _count = count;
        }

        public bool MoveNext()
        {
            if (_index < _count)
            {
                _current = _values[_index++];
                return true;
            }

            return false;
        }
        
        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }
        
        void IEnumerator.Reset()
        {
            Reset();
        }

        public void Reset()
        {
            _index = 0;
        }
        
        public T Current
        {
            get { return _current; }
        }

        T IEnumerator<T>.Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return _current; }
        }
        public void   Dispose() { }

        readonly T[] _values;
        T            _current;
        int          _index;
        readonly uint          _count;
    }
}