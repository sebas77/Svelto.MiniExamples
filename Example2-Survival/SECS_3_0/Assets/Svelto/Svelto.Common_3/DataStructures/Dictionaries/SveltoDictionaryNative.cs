using System;
using Svelto.Common;

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
    public struct SveltoDictionaryNative<TKey, TValue> : ISveltoDictionary<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        public SveltoDictionaryNative(uint size) : this(size, Allocator.Persistent) { }

        public SveltoDictionaryNative(uint size, Allocator nativeAllocator)
        {
            _dictionary =
                new SveltoDictionary<TKey, TValue, NativeStrategy<FasterDictionaryNode<TKey>>, NativeStrategy<TValue>>(
                    size, nativeAllocator);
        }

        public static implicit operator SveltoDictionaryNative<TKey, TValue>
            (SveltoDictionary<TKey, TValue, NativeStrategy<FasterDictionaryNode<TKey>>, NativeStrategy<TValue>> dic)
        {
            return new SveltoDictionaryNative<TKey, TValue>()
            {
                _dictionary = dic
            };
        }

        public NB<TValue> GetValues(out uint count)
        {
            count = this.count;
            return _dictionary._values.ToRealBuffer();
        }

        public uint count => _dictionary.count;

        public     void   Add(TKey key, in TValue value)           { _dictionary.Add(key, in value); }
        public     void   Set(TKey key, in TValue value)           { _dictionary.Set(key, in value); }
        public     void   Clear()                                  { _dictionary.Clear(); }
        public     void   FastClear()                              { _dictionary.FastClear(); }
        public     bool   ContainsKey(TKey key)                    { return _dictionary.ContainsKey(key); }
        public     bool   TryGetValue(TKey key, out TValue result) { return _dictionary.TryGetValue(key, out result); }
        public ref TValue GetOrCreate(TKey key)                    { return ref _dictionary.GetOrCreate(key); }

        public ref TValue GetOrCreate(TKey key, Func<TValue> builder)
        {
            return ref _dictionary.GetOrCreate(key, builder);
        }

        public ref TValue GetDirectValueByRef(uint index) { return ref _dictionary.GetDirectValueByRef(index); }
        public ref TValue GetValueByRef(TKey key)         { return ref _dictionary.GetValueByRef(key); }
        public     void   SetCapacity(uint size)          { _dictionary.SetCapacity(size); }
        public TValue this[TKey key] { get => _dictionary[key]; set => _dictionary[key] = value; }

        public bool Remove(TKey key)                           { return _dictionary.Remove(key); }
        public void Trim()                                     { _dictionary.Trim(); }
        public bool TryFindIndex(TKey key, out uint findIndex) { return _dictionary.TryFindIndex(key, out findIndex); }
        public uint GetIndex(TKey key)                         { return _dictionary.GetIndex(key); }

        public void Dispose() { _dictionary.Dispose(); }

        SveltoDictionary<TKey, TValue, NativeStrategy<FasterDictionaryNode<TKey>>, NativeStrategy<TValue>> _dictionary;
    }
}