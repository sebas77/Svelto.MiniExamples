using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public readonly ref struct LocalFasterReadOnlyList<T> 
    {
        public int count => (int) _count;

        public LocalFasterReadOnlyList(FasterList<T> list)
        {
            _list = list.ToArrayFast(out _count);
        }

        LocalFasterReadOnlyList(T[] list)
        {
            _list  = list;
            _count = (uint) list.Length;
        }
        
        public static implicit operator LocalFasterReadOnlyList<T>(FasterList<T> list)
        {
            return new LocalFasterReadOnlyList<T>(list);
        }
        
        public static implicit operator LocalFasterReadOnlyList<T>(T[] list)
        {
            return new LocalFasterReadOnlyList<T>(list);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LocalFasterReadonlyListEnumerator<T> GetEnumerator()
        {
            return new LocalFasterReadonlyListEnumerator<T>(_list, count);
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list[index];
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list[index];
        }

        readonly T[]  _list;
        readonly uint _count;
    }

    public struct LocalFasterReadonlyListEnumerator<T>
    {
        internal LocalFasterReadonlyListEnumerator(T[] list, int count)
        {
            _list  = list;
            _count = count;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _count;
        }
        public void Reset()    {  }
        public T    Current    => _list[_index];

        public void Dispose() { }
        
        readonly T[] _list;
        readonly int _count;
        int          _index;
    }
}