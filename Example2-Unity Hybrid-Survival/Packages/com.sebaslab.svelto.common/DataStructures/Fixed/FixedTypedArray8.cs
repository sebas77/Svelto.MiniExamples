#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using DBC.Common;

//todo needs to be unit tested
public struct FixedTypedArray8<T> where T : unmanaged
{
    static readonly int Length = 8;

    FixedTypedArray4<T> foursA;
    FixedTypedArray4<T> foursB;
    
    public int length => Length;
    
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            unsafe
            {
                 if (index >= Length)
                                    throw new Exception("out of bound index");
                
                fixed (FixedTypedArray8<T>* thisPtr = &this)
                {
                    var source = Unsafe.Add<T>(thisPtr, index);
                    ref var asRef = ref Unsafe.AsRef<T>(source);
                    return ref asRef;
                }
            }
        }
    }

    public FixedTypeEnumerator GetEnumerator()
    {
        return new FixedTypeEnumerator(ref Unsafe.AsRef<T>(foursA[0]));
    }
    
    public ref struct FixedTypeEnumerator
    {
        public FixedTypeEnumerator(ref T fixedTypedArray8):this()
        {
            unsafe
            {
                _fixedTypedArray8 = Unsafe.AsPointer(ref fixedTypedArray8);
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
        
        public ref T Current
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<FixedTypedArray8<T>>(_fixedTypedArray8)[_index];
                }
            }
        }
        
        readonly unsafe void * _fixedTypedArray8;
        int _index;
    }
}
#endif