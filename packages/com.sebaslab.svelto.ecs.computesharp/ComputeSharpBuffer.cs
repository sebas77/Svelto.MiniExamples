#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif
using System;
using System.Runtime.CompilerServices;
using ComputeSharp;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct ComputeSharpBuffer<T>:IBuffer<T> where T:unmanaged
    {
        public ComputeSharpBuffer(in UploadBuffer<T> array, ReadWriteBuffer<T> readWritebuffer) : this()
        {
            _readWritebuffer = readWritebuffer;
            _uploadBuffer    = array;
        }

        public void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint count)
        {
            _uploadBuffer.Span.Slice((int)sourceStartIndex, (int)count)
                         .CopyTo(destination.AsSpan((int)destinationStartIndex, (int)count));
        }
        
        public void Clear()
        {
            _uploadBuffer.Span.Clear();
            _uploadBuffer.CopyTo(_readWritebuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeArray(out int capacity)
        {
            capacity = (int)_uploadBuffer.Length;
            throw new NotSupportedException(
                "ComputeSharpBuffer does not expose a stable native pointer. Use GPU buffers directly or copy to managed memory.");
        }
        
        public ReadWriteBuffer<T> ToComputeBuffer()
        {
            // Upload CPU-side staging data before exposing the GPU buffer.
            _uploadBuffer.CopyTo(_readWritebuffer);

            return _readWritebuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadWriteBuffer<T> AsComputeBuffer()
        {
            // Use when the shader fully overwrites this buffer and no CPU->GPU upload is needed.
            return _readWritebuffer;
        }
        
        public void ReadBack()
        {
            _readWritebuffer.CopyTo(_uploadBuffer.Span);
        }

        public int capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int) _uploadBuffer.Length;
        }

        public bool isValid => _uploadBuffer != null;

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (index >= _uploadBuffer.Length)
                    throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                return ref _uploadBuffer.Span[(int)index];
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if ENABLE_DEBUG_CHECKS
                if (index < 0 || index >= _uploadBuffer.Length)
                    throw new Exception($"NativeBuffer - out of bound access: index {index} - capacity {capacity}");
#endif
                return ref _uploadBuffer.Span[index];
            }
        }

        //todo: maybe I should do this for the other buffers too?
        internal void Dispose()
        {
            _uploadBuffer.Dispose();
            _readWritebuffer.Dispose();
        }
        
        readonly UploadBuffer<T> _uploadBuffer;
        readonly ReadWriteBuffer<T> _readWritebuffer;
    }
}
