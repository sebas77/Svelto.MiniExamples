#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct SveltoStream
    {
        public SveltoStream(int sizeInByte): this()
        {
            capacity = sizeInByte;
        }

        public readonly int capacity;
        public int count => _writeCursor;
        public int space => capacity - _writeCursor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(in Span<byte> source) where T : unmanaged
        {
            int elementSize = MemoryUtilities.SizeOf<T>();
            int readCursor = _readCursor;

#if DEBUG && !PROFILE_SVELTO
            if (readCursor + elementSize > capacity)
                throw new Exception("no reading authorized");
#endif
            _readCursor += elementSize;

            return ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), readCursor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeRead<T>(ref T item, Span<byte> source, int size) where T : struct
        {
#if DEBUG && !PROFILE_SVELTO
            if (_readCursor + size > capacity)
                throw new Exception("no reading authorized");
            if (size > Unsafe.SizeOf<T>())
                throw new Exception("size is bigger than struct");
#endif
            Unsafe.CopyBlockUnaligned(
                ref Unsafe.As<T, byte>(ref item),
                ref Unsafe.Add(ref MemoryMarshal.GetReference(source), _readCursor),
                (uint)size); //size is not the size of T

            _readCursor += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<byte> destinationSpan, in T value) where T : unmanaged
        {
            int elementSize = MemoryUtilities.SizeOf<T>();

#if DEBUG && !PROFILE_SVELTO
            if (_writeCursor + elementSize > capacity)
                throw new Exception("no writing authorized");
#endif
            Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(destinationSpan), _writeCursor)) = value;

            _writeCursor += elementSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeWrite<T>(Span<byte> destinationSpan, in T item, int size) where T : struct
        {
#if DEBUG && !PROFILE_SVELTO
            if (_writeCursor + size > capacity)
                throw new Exception("no writing authorized");
            if (size > Unsafe.SizeOf<T>())
                throw new Exception("size is bigger than struct");
#endif
            //T can contain managed elements, it's up to the user to be sure that the right data is written
            //I cannot use span for this reason
            Unsafe.CopyBlockUnaligned(
                ref Unsafe.Add(ref MemoryMarshal.GetReference(destinationSpan), _writeCursor),
                ref Unsafe.As<T, byte>(ref Unsafe.AsRef(item)), (uint)size);

            _writeCursor += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(in Span<byte> destinationSpan, in Span<T> valueSpan) where T : unmanaged
        {
            int elementSize = MemoryUtilities.SizeOf<T>();
            int spanBytesToSerialise = elementSize * valueSpan.Length;

            //serialise the length of the span in bytes
            Write(destinationSpan, spanBytesToSerialise);

#if DEBUG && !PROFILE_SVELTO
            if (space < spanBytesToSerialise)
                throw new Exception("no writing authorized");
#endif
            if (spanBytesToSerialise > 0)
            {
                //create a local span of the destination from the right offset. 
                var destination = destinationSpan.Slice(_writeCursor, spanBytesToSerialise);
                valueSpan.CopyTo(MemoryMarshal.Cast<byte, T>(destination));

                _writeCursor += spanBytesToSerialise;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _writeCursor = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _readCursor = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAdvance()
        {
            return _readCursor < capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAdvance<T>()
                where T : unmanaged
        {
            int elementSize = MemoryUtilities.SizeOf<T>();

            return _readCursor + elementSize < capacity;
        }

        public int AdvanceCursor(int sizeOf)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_readCursor + sizeOf > capacity)
                throw new Exception("can't advance cursor, end of stream reached");
#endif
            var readCursor = _readCursor;
            _readCursor += sizeOf;

            return readCursor;
        }

        int _writeCursor;
        int _readCursor;
    }
}
#endif