using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public struct FasterReadOnlyList<T> 
    {
        public static FasterReadOnlyList<T> DefaultEmptyList = new FasterReadOnlyList<T>(
            FasterList<T>.DefaultEmptyList);

        public uint count      => _list.count;
        public bool IsReadOnly => true;
        public uint Capacity => _list.capacity;

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
        public T[] ToArrayFast(out uint count)
        {
            return _list.ToArrayFast(out count);
        }

        readonly FasterList<T> _list;
    }
}