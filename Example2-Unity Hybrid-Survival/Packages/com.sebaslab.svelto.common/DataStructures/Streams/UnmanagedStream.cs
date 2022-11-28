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
        public int count => (int) _writeCursor;
        public int space => (int)((int) capacity - _writeCursor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(in Span<byte> ptr) where T : unmanaged
        {
            unsafe
            {
                int sizeOf = MemoryUtilities.SizeOf<T>();
                int readCursor = _readCursor;

#if DEBUG && !PROFILE_SVELTO              
                if (readCursor + sizeOf > ptr.Length)
                    throw new Exception("no reading authorized");
#endif          
                _readCursor += sizeOf;

                return Unsafe.AsRef<T>(Unsafe.AsPointer(ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), readCursor))); //returning a copy so it's safe
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(in Span<byte> ptr, in T value) where T : unmanaged
        {
            unsafe
            {
                int sizeOf = MemoryUtilities.SizeOf<T>();
                
#if DEBUG && !PROFILE_SVELTO                
                if (_writeCursor + sizeOf > capacity)
                    throw new Exception("no writing authorized");
#endif
                Unsafe.AsRef<T>(Unsafe.AsPointer(ref Unsafe.Add(ref MemoryMarshal.GetReference(ptr), _writeCursor))) = value;

                _writeCursor += sizeOf;
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

    public struct UnmanagedStream
    {
        public unsafe UnmanagedStream(byte* ptr, int sizeInByte):this()
        {
            _ptr = ptr;
            _sveltoStream = new SveltoStream(sizeInByte);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged => _sveltoStream.Read<T>(ToSpan());

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
        public Span<byte> ToSpan()
        {
            unsafe
            {
                return new Span<byte>(_ptr, _sveltoStream.capacity);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AdvanceCursor(int sizeOf) => _sveltoStream.AdvanceCursor(sizeOf);

        SveltoStream _sveltoStream; //CANNOT BE READ ONLY

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST    
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly unsafe byte* _ptr;

    }
    
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