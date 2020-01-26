using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// todo: while at the moment is not strictly necessary, I will probably need to redesign this struct so that it can be shared over the time, otherwise it should be used as a ref struct (which is not possible) 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public unsafe struct NativeFasterDictionaryStruct<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        internal NativeFasterDictionaryStruct(int[] bufferBuckets, 
                                              TValue[] bufferValue, FasterDictionaryNode<TKey>[] bufferNode,
                                              GCHandle sentinel) : this()
        {
            _valuesInfo = GCHandle.Alloc(bufferNode, GCHandleType.Pinned);
            _values = GCHandle.Alloc(bufferValue, GCHandleType.Pinned);
            _buckets = GCHandle.Alloc(bufferBuckets, GCHandleType.Pinned);

            _valuesPointer = _values.AddrOfPinnedObject();
            _valuesInfoPointer = _valuesInfo.AddrOfPinnedObject();
            _bucketsPointer = _buckets.AddrOfPinnedObject();

            _bucketsSize = bufferBuckets.Length;
            _freeValueCellIndex = (uint) bufferValue.Length;
        }

        public void Dispose()
        {
            if (_values.IsAllocated)
                _values.Free();
            if (_valuesInfo.IsAllocated)
                _valuesInfo.Free();
            if (_buckets.IsAllocated)
                _buckets.Free();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue* GetValuesArray(out uint count)
        {
            count = _freeValueCellIndex;

            return (TValue*) _valuesPointer;
        }

        public TValue* unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (TValue*) _valuesPointer;
        }

        public uint Count => _freeValueCellIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            if (TryFindIndex(key, out _))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                result = unsafeValues[(int) findIndex];
                return true;
            }

            result = default;
            return false;
        }
        
        public ref TValue GetDirectValue(uint findIndex)
        {
            return ref ((TValue *)_valuesPointer)[findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref unsafeValues[(int) findIndex];
            }

            throw new FasterDictionaryException("Key not found");
        }

        public ref TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref unsafeValues[(int) GetIndex(key)];
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1
        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            int hash = Hash(key);
            int* gcHandle = (int*) _bucketsPointer;
            uint bucketIndex = Reduce((uint) hash, (uint) _bucketsSize);

            int valueIndex = gcHandle[bucketIndex] - 1;
            void* valuesInfo = (void*) _valuesInfoPointer;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                //for some reason this is way faster than using Comparer<TKey>.default, should investigate
                ref var valueInfo =
                    ref Unsafe.AsRef<FasterDictionaryNode<TKey>>(
                        (Unsafe.Add<FasterDictionaryNode<TKey>>(valuesInfo, valueIndex)));
                if (valueInfo.hashcode == hash && valueInfo.key.Equals(key) == true)
                {
                    //this is the one
                    findIndex = (uint) valueIndex;
                    return true;
                }

                valueIndex = valueInfo.previous;
            }

            findIndex = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint GetIndex(TKey key)
        {
            if (TryFindIndex(key, out var findIndex)) return findIndex;

            throw new FasterDictionaryException("Key not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Hash(TKey key)
        {
            return key.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Reduce(uint x, uint N)
        {
            if (x >= N)
                return x % N;

            return x;
        }

#if ENABLE_BURST_AOT        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr        _valuesPointer;
#if ENABLE_BURST_AOT        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr        _valuesInfoPointer;
#if ENABLE_BURST_AOT        
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        readonly IntPtr _bucketsPointer;
        readonly GCHandle _values;
        readonly GCHandle _valuesInfo;
        readonly GCHandle _buckets;
        readonly int      _bucketsSize;
        readonly uint     _freeValueCellIndex;
    }
}