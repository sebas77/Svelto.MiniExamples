#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;

//todo needs to be unit tested
public struct FixedTypedArray16<T> where T : unmanaged
{
    static readonly int Length = 16;

    FixedTypedArray8<T> eightsA;
    FixedTypedArray8<T> eightsB;

    public int length => Length;

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            unsafe
            {
                if (index >= Length)
                    throw new Exception("out of bound index");

                void* thisPtr = Unsafe.AsPointer(ref this);
                var source = Unsafe.Add<T>(thisPtr, index);
                ref var asRef = ref Unsafe.AsRef<T>(source);
                return asRef; //must return a copy to be safe
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            unsafe
            {
                if (index >= Length)
                    throw new Exception("out of bound index");

                void* thisPtr = Unsafe.AsPointer(ref this);
                var source = Unsafe.Add<T>(thisPtr, index);
                Unsafe.AsRef<T>(source) = value;
            }
        }
    }

    public FixedTypeEnumerator GetEnumerator()
    {
        return new FixedTypeEnumerator(ref Unsafe.AsRef<T>(eightsA[0]));
    }

    public ref struct FixedTypeEnumerator
    {
        public FixedTypeEnumerator(ref T fixedTypedArray16): this()
        {
            unsafe
            {
                _fixedTypedArray16 = Unsafe.AsPointer(ref fixedTypedArray16);
                _index = -1;
            }
        }

        public bool MoveNext()
        {
            if (_index < Length - 1)
            {
                _index++;

                return true;
            }

            return false;
        }

        public T Current
        {
            get
            {
                unsafe
                {
                    return Unsafe.AsRef<FixedTypedArray8<T>>(_fixedTypedArray16)[_index];
                }
            }
        }

        readonly unsafe void* _fixedTypedArray16;
        int _index;
    }
}
#endif