using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public interface ISveltoDictionary<TKey, TValue>: IDisposable where TKey : IEquatable<TKey>
    {
        int            count { get; }
        void            Add(TKey key, in TValue value);
        void            Set(TKey key, in TValue value);
        void            Clear();
        void            FastClear();
        bool            ContainsKey(TKey key);

        bool       TryGetValue(TKey key, out TValue result);
        ref TValue GetOrCreate(TKey key);
        ref TValue GetOrCreate(TKey key, Func<TValue> builder);
        ref TValue GetDirectValueByRef(uint index);
        ref TValue GetValueByRef(TKey key);
        void       SetCapacity(uint size);

        TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        bool Remove(TKey key);
        void Trim();
        bool TryFindIndex(TKey key, out uint findIndex);
        uint GetIndex(TKey key);
    }
}