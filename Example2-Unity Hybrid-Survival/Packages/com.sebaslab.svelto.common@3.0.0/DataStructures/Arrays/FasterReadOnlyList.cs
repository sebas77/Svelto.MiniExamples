using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public readonly struct FasterReadOnlyList<T> 
    {
        public static FasterReadOnlyList<T> DefaultEmptyList = new FasterReadOnlyList<T>(
            FasterList<T>.DefaultEmptyList);

        public int count      => _list.count;
        public uint capacity => _list.capacity;

        public FasterReadOnlyList(FasterList<T> list)
        {
            _list = list;
        }
        
        public static implicit operator FasterReadOnlyList<T>(FasterList<T> list)
        {
            return new FasterReadOnlyList<T>(list);
        }
        
        public static implicit operator LocalFasterReadOnlyList<T>(FasterReadOnlyList<T> list)
        {
            return new LocalFasterReadOnlyList<T>(list._list);
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        internal readonly FasterList<T> _list;
    }
    
    public readonly ref struct LocalFasterReadOnlyList<T> 
    {
        public int  count    => (int) _count;

        public LocalFasterReadOnlyList(FasterList<T> list)
        {
            _list     = list.ToArrayFast(out _count);
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
}