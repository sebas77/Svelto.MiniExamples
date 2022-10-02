using System;
using System.Runtime.CompilerServices;
using DBC.Common;
using Svelto.Common;

namespace Svelto.DataStructures
{
    /// <summary>
    /// They are called strategy because they abstract the handling of the memory type used.
    /// Through the IBufferStrategy interface, external datastructure can use interchangeably native and managed memory. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ManagedStrategy<T> : IBufferStrategy<T>
    {
        public ManagedStrategy(uint size) : this() { Alloc(size); }

        public bool isValid => _buffer != null;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Alloc(uint size)
        {
            var b =  default(MB<T>);
            b.Set(new T[size]);
            _realBuffer = b;
            _buffer     = _realBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Alloc(uint size, Allocator allocator, bool clear)
        {
            var b =  default(MB<T>);
            b.Set(new T[size]);
            _realBuffer = b;
            _buffer     = _realBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint newSize, bool copyContent = true, bool memClear = true)
        {
            if (newSize != capacity)
            {
                var realBuffer = _realBuffer.ToManagedArray();
                if (copyContent == true)
                    Array.Resize(ref realBuffer, (int) newSize);
                else
                    realBuffer = new T[newSize];

                var b = default(MB<T>);
                b.Set(realBuffer);
                _realBuffer = b;
                _buffer     = _realBuffer;
            }
        }

        public IntPtr AsBytesPointer()
        {
            throw new NotImplementedException();
        }

        public void   SerialiseFrom(IntPtr bytesPointer)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShiftLeft(uint index, uint count)
        {
            Check.Require(index < capacity, "out of bounds index");
            Check.Require(count < capacity, "out of bounds count");
            
            if (count == index) return;
            
            Check.Require(count > index, "wrong parameters used");
            
            var managedArray = _realBuffer.ToManagedArray();
            
            Array.Copy(managedArray, index + 1, managedArray, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShiftRight(uint index, uint count)
        {
            Check.Require(index < capacity, "out of bounds index");
            Check.Require(count < capacity, "out of bounds count");

            if (count == index) return;
            
            Check.Require(count > index, "wrong parameters used");
            
            var managedArray = _realBuffer.ToManagedArray();
            
            Array.Copy(managedArray, index, managedArray, index + 1, count - index);
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _realBuffer.capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _realBuffer.Clear(); }

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

        public MB<T> ToRealBuffer() { return _realBuffer; }
        
        IBuffer<T> IBufferStrategy<T>.ToBuffer()
        {
            Check.Require(_buffer != null, "Buffer not found in expected state");
            
            return _buffer;
        }

        public void Dispose() {}
        
        IBuffer<T>  _buffer;
        MB<T>       _realBuffer;
    }
}