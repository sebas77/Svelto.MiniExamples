#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif
using System;
using System.Runtime.CompilerServices;
using ComputeSharp;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct ComputeSharpBuffer<T>:IBuffer<T> where T:unmanaged
    {
        public ComputeSharpBuffer(in UploadBuffer<T> array, ReadWriteBuffer<T> readWritebuffer) : this()
        {
            _readWritebuffer = readWritebuffer;
            _ptr      = array;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            throw new NotImplementedException();
//            for (int i = 0; i < count; i++)
//            {
//                destination[i] = this[i];
//            }
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray(out int capacity)
        {
            throw new NotImplementedException();
        }
        
        public ReadWriteBuffer<T> ToComputeBuffer()
        {
            _ptr.CopyTo(_readWritebuffer);

            return _readWritebuffer;
        }
        
        public void Complete()
        {
            _readWritebuffer.CopyTo(_ptr.Span);
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int) _ptr.Length;
        }

        public bool isValid => _ptr != null;

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (index >= _ptr.Length)
                    throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                return ref _ptr.Span[(int)index];
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (index < 0 || index >= _ptr.Length)
                    throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                return ref _ptr.Span[index];
            }
        }

        //todo: maybe I should do this for the other buffers too?
        internal void Dispose()
        {
            _ptr.Dispose();
            _readWritebuffer.Dispose();
        }
        
        readonly UploadBuffer<T> _ptr;
        readonly ReadWriteBuffer<T> _readWritebuffer;
    }
}
