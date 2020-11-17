using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct ManagedStrategy<T> : IBufferStrategy<T>
    {
        IBuffer<T> _buffer;
        MB<T> _realBuffer;

        public ManagedStrategy(uint size):this()
        {
            Alloc(size, Allocator.None);
        }

        public bool isValid => _buffer != null;

        public void Alloc(uint size, Allocator nativeAllocator)
        {
            MB<T> b = new MB<T>();
            b.Set(new T[size]);
            this._realBuffer = b;
            _buffer = null;
        }

        public void Resize(uint newCapacity, bool copyContent = true)
        {
            DBC.Common.Check.Require(newCapacity > 0, "Resize requires a size greater than 0");
            
            var realBuffer = _realBuffer.ToManagedArray();
            if (copyContent == true)
                Array.Resize(ref realBuffer, (int) newCapacity);
            else
            {
                realBuffer = new T[newCapacity];
            }

            MB<T> b = new MB<T>();
            b.Set(realBuffer);
            this._realBuffer = b;
            _buffer = null;
        }

        public int capacity => _realBuffer.capacity;

        public void Clear() => _realBuffer.Clear();
        public void FastClear() => _realBuffer.FastClear();

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public IBuffer<T> ToBuffer()
        {
            if (_buffer == null)
                _buffer = _realBuffer;
            
            return _buffer;
        }

        public Allocator allocationStrategy => Allocator.Managed;

        public void       Dispose() {  }

        public MB<T> ToRealBuffer()
        {
            return _realBuffer;
        }
    }
}