#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct SveltoStream
    {
        public readonly int capacity;
        public int count => _writeCursor;

        public SveltoStream(int sizeInByte): this()
        {
            capacity = sizeInByte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Read<T>(in Span<byte> source) where T : unmanaged
        {
            int elementSize = MemoryUtilities.SizeOf<T>();
            int readCursor = _readCursor;

            if (readCursor + elementSize > capacity)
            {
                throw new Exception("TRYING TO READ PAST THE END OF THE STREAM -- this is bad!");
            }

            _readCursor += elementSize;
            return ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), readCursor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeRead<T>(ref T item, Span<byte> source, int size) where T : struct
        {
            if (_readCursor + size > capacity)
            {
                throw new Exception("TRYING TO READ PAST THE END OF THE STREAM -- this is bad!");
            }

#if DEBUG
            if (size > Unsafe.SizeOf<T>())
            {
                throw new Exception("size is bigger than struct");
            }
#endif

            Unsafe.CopyBlockUnaligned(
                ref Unsafe.As<T, byte>(ref item),
                ref Unsafe.Add(ref MemoryMarshal.GetReference(source), _readCursor),
                (uint)size); // size is not the size of T

            _readCursor += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<byte> destinationSpan, in T value) where T : unmanaged
        {
            int size = MemoryUtilities.SizeOf<T>();
            if (_writeCursor + size > capacity)
            {
                throw new Exception("STREAM DOES NOT HAVE ENOUGH SPACE LEFT TO WRITE -- this is bad!");
            }

            Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(destinationSpan), _writeCursor)) = value;
            _writeCursor += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeWrite<T>(Span<byte> destinationSpan, in T item, int size) where T : struct
        {
            if (_writeCursor + size > capacity)
            {
                throw new Exception("STREAM DOES NOT HAVE ENOUGH SPACE LEFT TO WRITE -- this is bad!");
            }

#if DEBUG
            if (size > Unsafe.SizeOf<T>())
            {
                throw new Exception("size is bigger than struct");
            }
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

            // NOTE: we write a USHORT for the size to optimize deltas
            // serialise the length of the span in bytes
            ushort lengthToWrite = (ushort) (elementSize * valueSpan.Length);
            Write(destinationSpan, lengthToWrite);

            if (_writeCursor + lengthToWrite > capacity)
            {
                throw new Exception("STREAM DOES NOT HAVE ENOUGH SPACE LEFT TO WRITE THE SPAN -- this is bad!");
            }

            if (lengthToWrite > 0)
            {
                // create a local span of the destination from the right offset.
                Span<byte> destination = destinationSpan.Slice(_writeCursor, lengthToWrite);
                valueSpan.CopyTo(MemoryMarshal.Cast<byte, T>(destination));
                _writeCursor += lengthToWrite;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AdvanceCursor(int sizeOf)
        {
            if (_readCursor + sizeOf > capacity)
            {
                throw new Exception("STREAM DOES NOT HAVE ENOUGH CAPACITY to advance the cursor -- this is bad!");
            }

            int readCursor = _readCursor;
            _readCursor += sizeOf;
            return readCursor;
        }

        int _writeCursor;
        int _readCursor;
    }
}
#endif