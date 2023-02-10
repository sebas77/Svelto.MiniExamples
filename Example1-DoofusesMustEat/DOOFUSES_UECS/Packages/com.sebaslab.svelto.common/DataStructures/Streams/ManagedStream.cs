#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public struct ManagedStream
    {
        public ManagedStream(byte[] ptr, int sizeInByte):this()
        {
            _ptr = ptr;
            _sveltoStream = new SveltoStream(sizeInByte);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged => _sveltoStream.Read<T>(ToSpan());
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(in T item) where T : unmanaged => _sveltoStream.Read<T>(ToSpan());
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //T can contain managed elements, it's up to the user to be sure that the right data is read
        public void UnsafeRead<T>(ref T item, int size) where T:struct => _sveltoStream.UnsafeRead(ref item, ToSpan(), size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in T value) where T : unmanaged => _sveltoStream.Write(ToSpan(), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<T> valueSpan) where T : unmanaged => _sveltoStream.WriteSpan(ToSpan(), valueSpan);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _sveltoStream.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _sveltoStream.Reset();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAdvance() => _sveltoStream.CanAdvance();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> ToSpan() => _ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AdvanceCursor(int sizeOf) => _sveltoStream.AdvanceCursor(sizeOf);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToPTR() => _ptr;

        SveltoStream _sveltoStream; //CANNOT BE READ ONLY

        readonly byte[] _ptr;
    }
}
#endif