using System;

namespace Svelto.DataStructures
{
    public class SveltoDictionaryUint<TValue> : ISveltoDictionary<uint, TValue> where TValue:class
    {
        SveltoDictionary<uint, TValue, NativeStrategy<FasterDictionaryNode<uint>>, ManagedStrategy<TValue>> _sveltoDictionaryNativeImplementation;

        public SveltoDictionaryUint()
        {
            _sveltoDictionaryNativeImplementation = new SveltoDictionary<uint, TValue, NativeStrategy<FasterDictionaryNode<uint>>, ManagedStrategy<TValue>>();
        }
        public uint count => _sveltoDictionaryNativeImplementation.count;

        public void Add(uint key, in TValue value) { _sveltoDictionaryNativeImplementation.Add(key, in value); }
        public void Set(uint key, in TValue value) { _sveltoDictionaryNativeImplementation.Set(key, in value); }
        public void Clear() { _sveltoDictionaryNativeImplementation.Clear(); }
        public void FastClear() { _sveltoDictionaryNativeImplementation.FastClear(); }
        public bool ContainsKey(uint key) { return _sveltoDictionaryNativeImplementation.ContainsKey(key); }
        public bool TryGetValue(uint key, out TValue result) { return _sveltoDictionaryNativeImplementation.TryGetValue(key, out result); }
        public ref TValue GetOrCreate(uint key) { return ref _sveltoDictionaryNativeImplementation.GetOrCreate(key); }
        public ref TValue GetOrCreate(uint key, Func<TValue> builder) { return ref _sveltoDictionaryNativeImplementation.GetOrCreate(key, builder); }
        public ref TValue GetDirectValueByRef(uint index) { return ref _sveltoDictionaryNativeImplementation.GetDirectValueByRef(index); }
        public ref TValue GetValueByRef(uint key) { return ref _sveltoDictionaryNativeImplementation.GetValueByRef(key); }
        public void SetCapacity(uint size) { _sveltoDictionaryNativeImplementation.SetCapacity(size); }
        public TValue this[uint key]
        {
            get => _sveltoDictionaryNativeImplementation[key];
            set => _sveltoDictionaryNativeImplementation[key] = value;
        }

        public bool Remove(uint key) { return _sveltoDictionaryNativeImplementation.Remove(key); }
        public void Trim() { _sveltoDictionaryNativeImplementation.Trim(); }
        public bool TryFindIndex(uint key, out uint findIndex) { return _sveltoDictionaryNativeImplementation.TryFindIndex(key, out findIndex); }
        public uint GetIndex(uint key) { return _sveltoDictionaryNativeImplementation.GetIndex(key); }
        void IDisposable.Dispose() { (_sveltoDictionaryNativeImplementation).Dispose(); }
    }
}