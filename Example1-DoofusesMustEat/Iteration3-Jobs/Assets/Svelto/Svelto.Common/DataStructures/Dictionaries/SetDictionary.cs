using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    /// <summary>
    /// This dictionary has been created for just one reason: I needed a dictionary that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard dictionary. Faster dictionary is also faster than
    /// the standard dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class SetDictionary<TValue>
    {
        public SetDictionary(uint size)
        {
            _values = new TValue[size];
            _buckets = new int[size];
        }
        
        public SetDictionary()
        {
            _values = new TValue[1];
            _buckets = new int[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] GetValuesArray(out uint count)
        {
            count = _freeValueCellIndex;

            return _values;
        }

        public TValue[] unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values;
        }

        public uint Count => _freeValueCellIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TValue value)
        {
            if (AddValue(_freeValueCellIndex, value) == false)
                throw new FasterDictionaryException("Key already present");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint key, in TValue value)
        {
            DBC.Common.Check.Require(key < _freeValueCellIndex, "key out of bound");

            if (_buckets[key] == key)
                _values[key] = value;
            else
            {
                AddValue(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_freeValueCellIndex == 0) return;

            _freeValueCellIndex = 0;

            Array.Clear(_buckets, 0, _buckets.Length);
            Array.Clear(_values, 0, _values.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            if (_freeValueCellIndex == 0) return;

            _freeValueCellIndex = 0;

            Array.Clear(_buckets, 0, _buckets.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint key)
        {
            return TryFindIndex(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                result = _values[(int) findIndex];
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint key)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int) findIndex];
            }

            throw new FasterDictionaryException("Key not found");
        }

        public ref TValue this[uint key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _values[(int) GetIndex(key)];
        }

        bool AddValue(uint key, in TValue value)
        {
            if (key == _values.Length)
            {
                var expandPrime = HashHelpers.ExpandPrime((int) _values.Length);

                Array.Resize(ref _values, expandPrime);
            }
            
            if (key == _buckets.Length)
            {
                var expandPrime = HashHelpers.ExpandPrime((int) _buckets.Length);

                Array.Resize(ref _buckets, expandPrime);
            }

            _values[_freeValueCellIndex] = value;
            _buckets[key] = (int) ++_freeValueCellIndex;

            return true;
        }

        public bool Remove(uint key)
        {
            if (key >= _freeValueCellIndex) return false;
            
            int indexToValueToRemove = _buckets[key] - 1;
            
            if (indexToValueToRemove == -1)
                return false; //not found!

            _buckets[key] = 0;
            
            _freeValueCellIndex--; //one less value to iterate

            if (indexToValueToRemove != _freeValueCellIndex)
            {
                _buckets[_freeValueCellIndex] = (int) (indexToValueToRemove + 1);

                _values[indexToValueToRemove] = _values[(int) _freeValueCellIndex];
            }

            return true;
        }

        public void Trim()
        {
            Array.Resize(ref _values, (int) _freeValueCellIndex);
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1
        public bool TryFindIndex(uint key, out uint findIndex)
        {
            int valueIndex = _buckets[key] - 1;

            //even if we found an existing value we need to be sure it's the one we requested
            if (valueIndex != -1)
            {
                    //this is the one
                    findIndex = (uint) valueIndex;
                    return true;
            }

            findIndex = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValue(uint index)
        {
            return ref _values[(int) index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint key)
        {
            if (TryFindIndex(key, out var findIndex)) return findIndex;

            throw new FasterDictionaryException("Key not found");
        }
        
        public SetDictionaryKeyValueEnumerator GetEnumerator() => new SetDictionaryKeyValueEnumerator(this);

        TValue[] _values;
        int[]    _buckets;
        uint     _freeValueCellIndex;
        
        public struct SetDictionaryKeyValueEnumerator
        {
            public SetDictionaryKeyValueEnumerator(SetDictionary<TValue> dic) : this()
            {
                _dic   = dic;
                _index = -1;
                _count = (int) dic.Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if DEBUG && !PROFILER
                if (_count != _dic.Count)
                    throw new FasterDictionaryException("can't modify a dictionary during its iteration");
#endif
                while (++_index < _count - 1 && _dic._buckets[_index] != -1);

                if (_index < _count)
                    return true;

                _index = -1;

                return false;
            }

            public KeyValuePairFast Current => new KeyValuePairFast((uint) (_dic._buckets[_index] + 1), _dic._values[_dic._buckets[_index]+ 1]);

            readonly SetDictionary<TValue> _dic;
            readonly int                            _count;

            int _index;
        }
        
        public readonly ref struct KeyValuePairFast
        {
            readonly TValue _value;
            readonly uint    _key;

            public KeyValuePairFast(uint dicBucket, TValue dicValues) 
            { 
                _key = dicBucket;
                _value = dicValues;
            }

            public uint Key   => _key;
            public TValue Value => _value;
        }

        public NativeFasterDictionaryStruct<uint, TEntityStruct> ToNative<TEntityStruct>() where TEntityStruct : unmanaged, TValue { throw new NotImplementedException(); }
    }
}