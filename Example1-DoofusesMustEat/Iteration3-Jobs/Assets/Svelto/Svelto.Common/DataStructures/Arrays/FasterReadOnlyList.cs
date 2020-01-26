using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public struct FasterReadOnlyList<T> 
    {
        public static FasterReadOnlyList<T> DefaultEmptyList = new FasterReadOnlyList<T>(
            FasterList<T>.DefaultEmptyList);

        public uint  Count      => _list.count;
        public bool IsReadOnly => true;

        public FasterReadOnlyList(FasterList<T> list)
        {
            _list = list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterListEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArrayFast()
        {
            return _list.ToArrayFast();
        }

        readonly FasterList<T> _list;
    }
}