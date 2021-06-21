using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Utilities;

namespace Svelto.DataStructures
{
    /// <summary>
    /// This dictionary has been created for just one reason: I needed a dictionary that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard dictionary. Faster dictionary is also faster than
    /// the standard dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one
    /// note: use native memory? Use _valuesInfo only when there are collisions?
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class FasterDictionary<TKey, TValue> where TKey : struct, IEquatable<TKey>
    {
        public FasterDictionary() : this(1) { }

        public FasterDictionary(uint size)
        {
            var dictionary =
                new SveltoDictionary<TKey, TValue, ManagedStrategy<SveltoDictionaryNode<TKey>>, ManagedStrategy<TValue>,
                    ManagedStrategy<int>>(size, Allocator.Persistent);

            _dictionary = dictionary;
        }
        
        public static FasterDictionary<TKey, TValue> Construct()
        {
            return new FasterDictionary<TKey, TValue>(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MB<TValue> GetValues(out uint count)
        {
            count = (uint) this.count;
            return _dictionary._values.ToRealBuffer();
        }

        public TValue[] unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary._values.ToRealBuffer().ToManagedArray();
        }

        public SveltoDictionaryNode<TKey>[] unsafeKeys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary._valuesInfo.ToRealBuffer().ToManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SveltoDictionaryKeyValueEnumerator<TKey, TValue, ManagedStrategy<SveltoDictionaryNode<TKey>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.count;
        }

        public bool isValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary.isValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value) { _dictionary.Add(key, in value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value) { _dictionary.Set(key, in value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _dictionary.Clear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear() { _dictionary.FastClear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) { return _dictionary.ContainsKey(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue result) { return _dictionary.TryGetValue(key, out result); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key) { return ref _dictionary.GetOrCreate(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key, Func<TValue> builder)
        {
            return ref _dictionary.GetOrCreate(key, builder);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate<W>(TKey key, FuncRef<W, TValue> builder, ref W parameter)
        {
            return ref _dictionary.GetOrCreate(key, builder, ref parameter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint index) { return ref _dictionary.GetDirectValueByRef(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key) { return ref _dictionary.GetValueByRef(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size) { _dictionary.SetCapacity(size); }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dictionary[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _dictionary[key] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key) { return _dictionary.Remove(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim() { _dictionary.Trim(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(TKey key, out uint findIndex) { return _dictionary.TryFindIndex(key, out findIndex); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(TKey key) { return _dictionary.GetIndex(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() { _dictionary.Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyValuesTo(FasterList<TValue> values)
        {
            values.AddRange(GetValues(out var count).ToManagedArray(), count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyValuesTo(TValue[] values, uint index = 0)
        {
            Array.Copy(GetValues(out var count).ToManagedArray(), 0, values, index, count);
        }

#if UNITY_COLLECTIONS
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif

        SveltoDictionary<TKey, TValue, ManagedStrategy<SveltoDictionaryNode<TKey>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> _dictionary;
    }
}