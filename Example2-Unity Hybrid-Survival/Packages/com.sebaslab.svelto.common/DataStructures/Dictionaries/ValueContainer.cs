using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.DataStructures.Experimental
{
    ///
    /// A set is just about knowing if an entity identified through an id exists or doesn't exist in the set
    /// that's why bitsets can be used, it's just about true or false, there is or there isn't in the set.
    /// in ECS we can have a set for each component and list what entities are in that set
    ///
    /// entities: 0, 1, 2, 3, 4, 5, 6
    ///
    /// component1: 1, 5, 6
    /// component2: 2, 3, 5
    ///
    /// it's possible to intersect bitsets to know the entities that have all the components shared
    ///
    /// The following class is not a sparse set, it's more an optimised dictionary for cases where the user
    /// cannot decide the key value.
    /// 
    public sealed class ValueContainer<T, StrategyD, StrategyS> where StrategyD : IBufferStrategy<T>, new()
        where StrategyS : IBufferStrategy<int>, new()
    {
        public ValueContainer()
        {
            _sparse = new StrategyS();
            _sparse.Alloc(1, Allocator.Persistent, false);
            _sparse[0] = 0;
            _dense = new StrategyD();
            _dense.Alloc(1, Allocator.Persistent, false);
        }

        public ValueContainer(uint size)
        {
            _sparse = new StrategyS();
            _sparse.Alloc(size, Allocator.Persistent, true);
            _dense = new StrategyD();
            _dense.Alloc(size, Allocator.Persistent, false);
        }

        public int capacity => _dense.capacity;
        public int count => (int)_count;

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= count)
                    throw new Exception($"SparseSet - out of bound access: index {count} - capacity {count}");
#endif
                return ref _dense[index];
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG && !PROFILE_SVELTO
                if (index >= count)
                    throw new Exception($"SparseSet - out of bound access: index {count} - capacity {count}");
#endif
                return ref _dense[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
            
            _dense.Clear();
            _sparse.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint index)
        {
            return index < capacity && _sparse[index] < count && _sparse[index] >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(T val)
        {
            uint index;
            if (_lastFreeIndex == 0)
            {
                index = (uint)count;
                if (index >= capacity)
                    Reserve((uint)Math.Ceiling(capacity * 1.5f));
                ++_count;
            }
            else
            {
                index = _lastFreeIndex;
                _lastFreeIndex = (uint)-_sparse[index];
            }

            _dense[index] = val;
            _sparse[index] = (int)index;

            return (uint)index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint index)
        {
            var denseIndex = _sparse[index]; //from index to the index in dense
            _dense[denseIndex] = _dense[_count - 1]; //move the value to the index to replace
            _sparse[count - 1] = denseIndex;
            _sparse[index] = (int)-_lastFreeIndex;
            _lastFreeIndex = (uint)denseIndex;
            --_count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(uint u)
        {
            if (u > capacity)
            {
                _dense.Resize(u);
                _sparse.Resize(u);
            }
        }

        ~ValueContainer()
        {
            _sparse.Dispose();
            _dense.Dispose();
        }

        readonly StrategyD _dense;  //Dense set of elements
        readonly StrategyS _sparse; //Map of elements to dense set indices //Should this be a bitset?
        uint _count;
        uint _lastFreeIndex;
    }
}