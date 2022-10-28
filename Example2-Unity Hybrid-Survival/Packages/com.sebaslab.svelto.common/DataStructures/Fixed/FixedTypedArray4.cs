#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using System.Runtime.CompilerServices;

//todo needs to be unit tested
public struct FixedTypedArray4<T> where T : unmanaged
{
    static readonly int Length = 4;
    
    T field0;
    T field1;
    T field2;
    T field3;

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
                
                fixed (FixedTypedArray4<T>* thisPtr = &this)
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
        return new FixedTypeEnumerator(ref Unsafe.AsRef<T>(field0));
    }
    
    public ref struct FixedTypeEnumerator
    {
        public FixedTypeEnumerator(ref T fixedTypedArray4):this()
        {
            unsafe
            {
                _fixedTypedArray4 = Unsafe.AsPointer(ref fixedTypedArray4);
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
                    return ref Unsafe.AsRef<FixedTypedArray4<T>>(_fixedTypedArray4)[_index];
                }
            }
        }
        
        readonly unsafe void * _fixedTypedArray4;
        int _index;
    }
}
#endif