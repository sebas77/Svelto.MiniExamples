namespace Svelto.DataStructures
{
    public ref struct FasterListEnumerator<T>
    {
        public ref T Current
        {
            get
            {
                DBC.Common.Check.Require(_counter <= _size);
                return ref _buffer[(uint)_counter - 1];
            }
        }

        public FasterListEnumerator(in T[] buffer, uint size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
        }

        public bool MoveNext()
        {
            if (_counter++ < _size)
                return true;

            return false;
        }

        public void Reset()
        {
            _counter = 0;
        }

        readonly T[]  _buffer;
        int           _counter;
        readonly uint _size;
    }
}