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
        public ref T Read<T>(in Span<byte> ptr) where T : unmanaged
        {
            int sizeOf = MemoryUtilities.SizeOf<T>();
            int readCursor = _readCursor;

#if DEBUG && !PROFILE_SVELTO
            if (readCursor + sizeOf > capacity)
                throw new Exception("no reading authorized");
#endif
            _readCursor += sizeOf;

            return ref Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), readCursor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T str, Span<byte> ptr, int size) where T : struct
        {
#if DEBUG && !PROFILE_SVELTO
            if (_readCursor + size > capacity)
                throw new Exception("no reading authorized");
            if (size > Unsafe.SizeOf<T>())
                throw new Exception("size is bigger than struct");
#endif
            Unsafe.CopyBlock(
                ref Unsafe.As<T, byte>(ref str),
                ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), _readCursor), (uint)size);
            
            _readCursor += size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<byte> ptr, in T value) where T : unmanaged
        {
            int sizeOf = MemoryUtilities.SizeOf<T>();

#if DEBUG && !PROFILE_SVELTO
            if (_writeCursor + sizeOf > capacity)
                throw new Exception("no writing authorized");
#endif
            Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), _writeCursor)) = value;

            _writeCursor += sizeOf;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(Span<byte> ptr, in T str, int size) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_writeCursor + size > capacity)
                    throw new Exception("no writing authorized");
                if (size > Unsafe.SizeOf<T>())
                    throw new Exception("size is bigger than struct");
#endif
                var pptr = Unsafe.AsPointer(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(str)));
            
                Unsafe.CopyBlock(
                    ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), _writeCursor),
                    ref Unsafe.As<T, byte>(ref Unsafe.AsRef(str)), (uint)size);

                _writeCursor += size;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(in Span<byte> ptr, in Span<T> valueSpan) where T : unmanaged
        {
            unsafe
            {
                int singleSizeOf = MemoryUtilities.SizeOf<T>();
                int sizeOf = singleSizeOf * valueSpan.Length;

                Write(ptr, sizeOf);

#if DEBUG && !PROFILE_SVELTO
                if (space < sizeOf)
                    throw new Exception("no writing authorized");
#endif

                var asPointer = Unsafe.AsPointer(ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), _writeCursor));
                Span<T> destination = new Span<T>(asPointer, space / singleSizeOf);
                valueSpan.CopyTo(destination);

                _writeCursor += sizeOf;
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

        public bool CanAdvance() => _readCursor < capacity;

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