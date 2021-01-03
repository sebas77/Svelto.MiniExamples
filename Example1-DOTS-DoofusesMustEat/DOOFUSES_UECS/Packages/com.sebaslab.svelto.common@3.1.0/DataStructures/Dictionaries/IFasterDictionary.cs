// using System;
// using System.Runtime.CompilerServices;
//
// namespace Svelto.DataStructures
// {
//     /// <summary>
//     /// This dictionary has been created for just one reason: I needed a dictionary that would have let me iterate
//     /// over the values as an array, directly, without generating one or using an iterator.
//     /// For this goal is N times faster than the standard dictionary. Faster dictionary is also faster than
//     /// the standard dictionary for most of the operations, but the difference is negligible. The only slower operation
//     /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
//     /// one
//     /// </summary>
//     /// <typeparam name="TKey"></typeparam>
//     /// <typeparam name="TValue"></typeparam>
//     
//     public interface IFasterDictionary<TKey, TValue> where TKey : IEquatable<TKey>
//     {
//         uint       count { get; }
//         void       Add(TKey key, in TValue value);
//         void       Set(TKey key, in TValue value);
//         void       Clear();
//         void       FastClear();
//         bool       ContainsKey(TKey   key);
//         bool       TryGetValue(TKey   key, out TValue result);
//         ref TValue GetOrCreate(TKey   key);
//         ref TValue GetOrCreate(TKey   key, System.Func<TValue> builder);
//         ref TValue GetValueByRef(TKey key);
//         void       SetCapacity(uint   size);
//
//         TValue this[TKey key]
//         {
//             [MethodImpl(MethodImplOptions.AggressiveInlining)]
//             get;
//             [MethodImpl(MethodImplOptions.AggressiveInlining)]
//             set;
//         }
//
//         bool       Remove(TKey key);
//         void       Trim();
//         bool       TryFindIndex(TKey   key, out uint findIndex);
//         uint       GetIndex(TKey       key);
//     }
// }