using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    /// it is confirmed that when using a bit set this operation is necessary components[denseset[i]]
    ///
    /// The following class is not a sparse set, it's more an optimised dictionary for cases where the user
    /// cannot decide the key value.
    /// 
    public struct ValueContainer<T, StrategyD, StrategyS>
            where StrategyD : IBufferStrategy<T>, new()
            where StrategyS : IBufferStrategy<SparseIndex>, new()
    {
        public ValueContainer(uint initialSize): this()
        {
            _sparse = new StrategyS();
            _sparse.Alloc(initialSize, Allocator.Persistent, true);
            _dense = new StrategyD();
            _dense.Alloc(initialSize, Allocator.Persistent, false);
        }

        public int capacity => _dense.capacity;
        public int count => (int)_count;

        public ref T this[ValueIndex index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                DBC.Common.Check.Require(Has(index) == true, $"SparseSet - invalid index: index {index}");

                return ref _dense[index.sparseIndex];
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
        public bool Has(ValueIndex index)
        {
            return index.version > 0
                 && index.sparseIndex < capacity
                 && index.version == _sparse[index.sparseIndex].version
                 && _sparse[index.sparseIndex].denseIndex < count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueIndex Add(T val)
        {
            if (_freeList.IsValid())
            {
                _dense[_freeList.denseIndex] = val;

                ValueIndex ret = new ValueIndex(_freeList.denseIndex, _freeList.version);

                _freeList = _freeList.Next();

                return ret;
            }
            var index = (uint)count;
            if (index >= capacity)
                Reserve((uint)Math.Ceiling((capacity + 1) * 1.5f));

            ++_count;

            _dense[index] = val;
            var version = (byte)(_sparse[index].version + 1);
            _sparse[index] = new SparseIndex(index, version); //base count is 1 so 0 can be used as invalid

            return new ValueIndex(index, version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ValueIndex index)
        {
            DBC.Common.Check.Require(Has(index) == true, $"SparseSet - invalid index: index {index}");
            
            ref var sparseIndex = ref _sparse[index.sparseIndex];
            
            //invalidate value index as the sparse index version will increment
            //store the current _freelist to create a list of free spots
            _sparse[index.sparseIndex] = new SparseIndex(sparseIndex, _freeList);
            //set the new free list to the just cleared spot
            _freeList = sparseIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(uint u)
        {
            DBC.Common.Check.Require(u < MAX_SIZE, "Max size reached");

            if (u > capacity)
            {
                _dense.Resize(u);
                _sparse.Resize(u);
            }
        }

        public void Dispose()
        {
            _sparse.Dispose();
            _dense.Dispose();
        }

        StrategyD _dense;  //Dense set of elements
        StrategyS _sparse; //Map of elements to dense set indices //Should this be a bitset?
        uint _count;
        SparseIndex _freeList;

        static int MAX_SIZE = (int)Math.Pow(2, 24);
        
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ValueIndex
    {
        internal uint sparseIndex => _sparseIndex & 0x00FFFFFF;
        internal byte version => _version;

        [FieldOffset(0)] readonly uint _sparseIndex;
        [FieldOffset(3)] readonly byte _version;

        public ValueIndex(uint index, byte ver)
        {
            _sparseIndex = index;
            _version = ver;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SparseIndex
    {
        internal uint denseIndex => _denseIndex & 0x00FFFFFF;
        internal byte version => _version;

        [FieldOffset(0)] readonly uint _denseIndex;
        [FieldOffset(3)] readonly byte _version;
        [FieldOffset(4)] readonly uint _nextFreeIndex;
        [FieldOffset(7)] readonly byte _nextFreeVer;

        public SparseIndex(uint index, byte ver):this()
        {
            _denseIndex = index;
            _version = ver;
        }

        internal SparseIndex(SparseIndex index, SparseIndex nextFree)
        {
            _denseIndex = index.denseIndex;
            _version = (byte)(index.version + 1);
            _nextFreeIndex = nextFree.denseIndex;
            _nextFreeVer = nextFree._version;
        }

        public bool IsValid()
        {
            return _version > 0;
        }

        public SparseIndex Next()
        {
            return new SparseIndex(_nextFreeIndex, _nextFreeVer);
        }
    }
}