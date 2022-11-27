#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.Common.DataStructures
{
    public readonly struct ByteArraySegment<T> where T : unmanaged
    {
        public ByteArraySegment(byte[] reference, int offsetInBytes, int sizeInBytes): this()
        {
            _byteReference = new Memory<byte>(reference, offsetInBytes, sizeInBytes);
        }

        ByteArraySegment(T[] reference): this()
        {
            _reference = reference;
        }

        public static implicit operator ByteArraySegment<T>(T[] list)
        {
            return new ByteArraySegment<T>(list);
        }

        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _reference == null ? MemoryMarshal.Cast<byte, T>(_byteReference.Span) : new Span<T>(_reference);
        }

        readonly Memory<byte> _byteReference;
        readonly T[] _reference;
    }

    public static class ByteArraySegmentExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteArraySegment<T> ReadByteArraySegment<T>(this ref ManagedStream stream) where T : unmanaged
        {
            int length = stream.Read<int>();

            int readCursor = (int)stream.AdvanceCursor(length);

            return new ByteArraySegment<T>(stream.ToPTR(), readCursor, length);
        }
    }
}
#endif