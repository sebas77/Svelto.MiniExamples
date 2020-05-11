#if SPAN //FOR TESTING/PROFILING PURPOSES ONLY
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.DataStructures
{
    class UnmanagedMemoryManager<T> : MemoryManager<T>
    {
        uint    _elementCount;
        IntPtr _ptr;
 
        public UnmanagedMemoryManager(uint elementCount)
        {
            _elementCount = elementCount;
 
            _ptr = Marshal.AllocHGlobal((int) (elementCount * Unsafe.SizeOf<T>()));
        }
        
        ~UnmanagedMemoryManager()
        {
            Dispose(false);
        }
        
        protected override void Dispose(bool disposing)
        { 
            if (_elementCount > 0) {
                Marshal.FreeHGlobal(_ptr);
                _elementCount = 0;
            }
        }
 
        public override Span<T> GetSpan()
        {
            unsafe
            {
                return new Span<T>((void*) _ptr, (int) _elementCount);
            }
        }
 
        public override MemoryHandle Pin(int elementIndex)
        {
            throw new NotImplementedException();
        }
 
        public override void Unpin()
        {
            throw new NotImplementedException();
        }

        public Memory<T> Resize(uint newSize)
        {
            unsafe
            {
                var sizeOf = (int) (newSize * Unsafe.SizeOf<T>());
                var ptr    = Marshal.AllocHGlobal(sizeOf);
                Unsafe.CopyBlock((void*) ptr, (void*) _ptr, (uint) sizeOf);
                Marshal.FreeHGlobal(_ptr);
                _ptr = ptr;
                _elementCount = newSize;

                return Memory;
            }
        }

        public void Free() { Dispose(true); }
    }
    
    class ManagedMemoryManager<T> : MemoryManager<T>
    {
        readonly T[] _ptr;
 
        public ManagedMemoryManager(int elementCount) { _ptr = new T[elementCount]; }
 
        protected override void Dispose(bool disposing)
        { }
 
        public override Span<T> GetSpan()
        {
            return new Span<T>(_ptr);
        }
 
        public override MemoryHandle Pin(int elementIndex)
        {
            throw new Exception();
        }
 
        public override void Unpin()
        {
            throw new Exception();
        }
    }
    
    public interface IAllocationStrategy
    {
        SveltoMemory<T> Allocate<T>(uint size);
    }

    public struct SveltoMemory<T>
    {
        public Span<T> Span => _memory.Span;
        public int Length => _memory.Length;

        Memory<T> _memory;
        public SveltoMemory(Memory<T> memory) { _memory = memory; }
        public Memory<T> memory => _memory;

        public void Resize(uint size)
        {
            if (MemoryMarshal.TryGetMemoryManager<T, UnmanagedMemoryManager<T>>(_memory, out var manager) == false)
            {
                var memory = new Memory<T>(new T[size]);
                _memory.Span.Slice(0, size > Length ? Length : (int) size).CopyTo(memory.Span);
                _memory = memory;
            }
            else
            {
                _memory = manager.Resize(size);
            }
        }

        public void Dispose()
        {
            if (MemoryMarshal.TryGetMemoryManager<T, UnmanagedMemoryManager<T>>(_memory, out var manager) == true)
            {
                manager.Free();
            }
        }

        public ref T this[int index]
        {
            get
            {
                //ref var firstElement = ref MemoryMarshal.GetReference(_memory.Span);
                //return ref Unsafe.Add(ref firstElement, index);
                return ref memory.Span[index];
            }
        }
    }

    public class ManagedAllocationStrategy : IAllocationStrategy
    {
        public SveltoMemory<T> Allocate<T>(uint size)
        {
            return new SveltoMemory<T>(new T[size]);
        }
    }
    
    public class NativeAllocationStrategy : IAllocationStrategy
    {
        public SveltoMemory<T> Allocate<T>(uint size)
        {
            return new SveltoMemory<T>(new UnmanagedMemoryManager<T>(size).Memory);
        }
    }
    
    public struct SpanDictionary<TKey, TValue> : IDisposable
        where TKey : IEquatable<TKey>
    {
        public SpanDictionary(uint size, IAllocationStrategy allocationStrategy):this()
        {
            _valuesInfo = new Node[size];
            _values = allocationStrategy.Allocate<TValue>(size);
            _buckets = new int[HashHelpers.GetPrime((int) size)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<TValue> GetValuesArray(out uint count)
        {
            count = _freeValueCellIndex;

            return _values.memory;
        }

        public Span<TValue> unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Span;
        }

        public uint Count => _freeValueCellIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value)
        {
            if (AddValue(key, in value, out _) == false)
                throw new FasterDictionaryException("Key already present");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value)
        {
            AddValue(key, in value, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_freeValueCellIndex == 0) return;

            _freeValueCellIndex = 0;

            Array.Clear(_buckets, 0, _buckets.Length);
            _values.Span.Clear();
            Array.Clear(_valuesInfo, 0, _valuesInfo.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            if (_freeValueCellIndex == 0) return;

            _freeValueCellIndex = 0;

            Array.Clear(_buckets, 0, _buckets.Length);
            Array.Clear(_valuesInfo, 0, _valuesInfo.Length);
        }

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
        public FasterDictionaryKeyValueEnumerator GetEnumerator()
        {
            return new FasterDictionaryKeyValueEnumerator(this);
        }
        
        public void Dispose() { _values.Dispose(); }

        public bool TryGetValue(TKey key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                result = _values[(int) findIndex];
                return true;
            }

            result = default;
            return false;
        }

        //todo: can be optimized
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key)
        {
            var valuesSpan = _values;
            
            if (TryFindIndex(key, out var findIndex))
            {
                return ref valuesSpan[(int) findIndex];
            }

            AddValue(key, default, out var findIndexA);

            return ref valuesSpan[(int) findIndexA];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(TKey key, System.Func<TValue> builder)
        {
            var valuesSpan = _values;
            if (TryFindIndex(key, out var findIndex))
            {
                return ref valuesSpan[(int) findIndex];
            }

            AddValue(key, builder(), out var findIndexA);

            return ref valuesSpan[(int) findIndexA];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
            if (TryFindIndex(key, out var findIndex))
            {
                return ref _values[(int) findIndex];
            }

            throw new FasterDictionaryException("Key not found");
        }

        public void SetCapacity(uint size)
        {
            if (_values.Length < size)
            {
                //Array.Resize(ref _values, (int) size);
                _values.Resize(size);
                Array.Resize(ref _valuesInfo, (int) size);
            }
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[(int) GetIndex(key)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AddValue(key, in value, out _);
        }

        bool AddValue(TKey key, in TValue value, out uint indexSet)
        {
            int hash = key.GetHashCode();
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.Length);

            if (_freeValueCellIndex == _values.Length)
            {
                var expandPrime = HashHelpers.ExpandPrime((int) _freeValueCellIndex);

                //Array.Resize(ref _values, expandPrime);
                _values.Resize((uint) expandPrime);
                Array.Resize(ref _valuesInfo, expandPrime);
            }

            //buckets value -1 means it's empty
            var valueIndex = _buckets[bucketIndex] - 1;

            if (valueIndex == -1)
                //create the info node at the last position and fill it with the relevant information
                _valuesInfo[_freeValueCellIndex] = new Node(ref key, hash);
            else //collision or already exists
            {
                {
                    int currentValueIndex = valueIndex;
                    do
                    {
                        //must check if the key already exists in the dictionary
                        //for some reason this is faster than using Comparer<TKey>.default, should investigate
                        if (_valuesInfo[currentValueIndex].hashcode == hash &&
                            _valuesInfo[currentValueIndex].key.Equals(key) == true)
                        {
                            //the key already exists, simply replace the value!
                            _values[currentValueIndex] = value;
                            indexSet = (uint) currentValueIndex;
                            return false;
                        }

                        currentValueIndex = _valuesInfo[currentValueIndex].previous;
                    } while (currentValueIndex != -1); //-1 means no more values with key with the same hash
                }

                //oops collision!
                _collisions++;
                //create a new node which previous index points to node currently pointed in the bucket
                _valuesInfo[_freeValueCellIndex] = new Node(ref key, hash, valueIndex);
                //update the next of the existing cell to point to the new one
                //old one -> new one | old one <- next one
                _valuesInfo[valueIndex].next = (int) _freeValueCellIndex;
                //Important: the new node is always the one that will be pointed by the bucket cell
                //so I can assume that the one pointed by the bucket is always the last value added
                //(next = -1)
            }

            //item with this bucketIndex will point to the last value created
            //ToDo: if instead I assume that the original one is the one in the bucket
            //I wouldn't need to update the bucket here. Small optimization but important
            _buckets[bucketIndex] = (int) (_freeValueCellIndex + 1);

            _values[(int) _freeValueCellIndex] = value;
            indexSet = (uint) _freeValueCellIndex;

            _freeValueCellIndex++;

            //too many collisions?
            if (_collisions > _buckets.Length)
            {
                //we need more space and less collisions
                _buckets = new int[HashHelpers.ExpandPrime((int) _collisions)];

                _collisions = 0;

                //we need to get all the hash code of all the values stored so far and spread them over the new bucket
                //length
                for (int newValueIndex = 0; newValueIndex < _freeValueCellIndex; newValueIndex++)
                {
                    //get the original hash code and find the new bucketIndex due to the new length
                    bucketIndex = Reduce((uint) _valuesInfo[newValueIndex].hashcode, (uint) _buckets.Length);
                    //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                    //we create a new node which prev points to the old one. Old one next points to the new one.
                    //the bucket will now points to the new one
                    //In this way we can rebuild the linkedlist.
                    //get the current valueIndex, it's -1 if no collision happens
                    int existingValueIndex = _buckets[bucketIndex] - 1;
                    //update the bucket index to the index of the current item that share the bucketIndex
                    //(last found is always the one in the bucket)
                    _buckets[bucketIndex] = newValueIndex + 1;
                    if (existingValueIndex != -1)
                    {
                        //oops a value was already being pointed by this cell in the new bucket list,
                        //it means there is a collision, problem
                        _collisions++;
                        //the bucket will point to this value, so 
                        //the previous index will be used as previous for the new value.
                        _valuesInfo[newValueIndex].previous = existingValueIndex;
                        _valuesInfo[newValueIndex].next = -1;
                        //and update the previous next index to the new one
                        _valuesInfo[existingValueIndex].next = newValueIndex;
                    }
                    else
                    {
                        //ok nothing was indexed, the bucket was empty. We need to update the previous
                        //values of next and previous
                        _valuesInfo[newValueIndex].next = -1;
                        _valuesInfo[newValueIndex].previous = -1;
                    }
                }
            }

            return true;
        }

        public bool Remove(TKey key)
        {
            int hash = Hash(key);
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.Length);

            //find the bucket
            int indexToValueToRemove = _buckets[bucketIndex] - 1;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                if (_valuesInfo[indexToValueToRemove].hashcode == hash &&
                    _valuesInfo[indexToValueToRemove].key.Equals(key) == true)
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (_buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
                        DBC.Common.Check.Require(_valuesInfo[indexToValueToRemove].next == -1,
                            "if the bucket points to the cell, next MUST NOT exists");
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order  
                        //                      B(ucket points always to the last one)
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        int value = _valuesInfo[indexToValueToRemove].previous;
                        _buckets[bucketIndex] = value + 1;
                    }
                    else
                        DBC.Common.Check.Require(_valuesInfo[indexToValueToRemove].next != -1,
                            "if the bucket points to another cell, next MUST exists");

                    UpdateLinkedList(indexToValueToRemove, _valuesInfo);

                    break;
                }

                indexToValueToRemove = _valuesInfo[indexToValueToRemove].previous;
            }

            if (indexToValueToRemove == -1)
                return false; //not found!

            _freeValueCellIndex--; //one less value to iterate

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still has got the value to delete. Remember the goal of this dictionary is to be able
            //to iterate over the values like an array, so the values array must always be up to date

            //if the cell to remove is the last one in the list, we can perform less operations (no swapping needed)
            //otherwise we want to move the last value cell over the value to remove
            if (indexToValueToRemove != _freeValueCellIndex)
            {
                //we can move the last value of both arrays in place of the one to delete.
                //in order to do so, we need to be sure that the bucket pointer is updated
                //first we find the index in the bucket list of the pointer that points to the cell
                //to move
                var movingBucketIndex =
                    Reduce((uint) _valuesInfo[_freeValueCellIndex].hashcode, (uint) _buckets.Length);

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved
                if (_buckets[movingBucketIndex] - 1 == _freeValueCellIndex)
                    _buckets[movingBucketIndex] = (int) (indexToValueToRemove + 1);

                //otherwise it means that there was more than one key with the same hash (collision), so 
                //we need to update the linked list and its pointers
                int next = _valuesInfo[_freeValueCellIndex].next;
                int previous = _valuesInfo[_freeValueCellIndex].previous;

                //they now point to the cell where the last value is moved into
                if (next != -1)
                    _valuesInfo[next].previous = (int) indexToValueToRemove;
                if (previous != -1)
                    _valuesInfo[previous].next = (int) indexToValueToRemove;

                //finally, actually move the values
                _valuesInfo[indexToValueToRemove] = _valuesInfo[_freeValueCellIndex];
                _values[indexToValueToRemove] = _values[(int) _freeValueCellIndex];
            }

            return true;
        }

        public void Trim()
        {
            //Array.Resize(ref _values, (int) _freeValueCellIndex);
            _values.Resize(_freeValueCellIndex);
            Array.Resize(ref _valuesInfo, (int) _freeValueCellIndex);
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1
        public bool TryFindIndex(TKey key, out int findIndex)
        {
            int hash = Hash(key);
            uint bucketIndex = Reduce((uint) hash, (uint) _buckets.Length);

            int valueIndex = _buckets[bucketIndex] - 1;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                //for some reason this is way faster than using Comparer<TKey>.default, should investigate
                if (_valuesInfo[valueIndex].hashcode == hash && _valuesInfo[valueIndex].key.Equals(key) == true)
                {
                    //this is the one
                    findIndex =  valueIndex;
                    return true;
                }

                valueIndex = _valuesInfo[valueIndex].previous;
            }

            findIndex = 0;
            return false;
        }

        public ref TValue GetDirectValue(uint index)
        {
            return ref _values[(int) index];
        }

        public uint GetIndex(TKey key)
        {
            if (TryFindIndex(key, out var findIndex)) return (uint) findIndex;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateLinkedList(int index, Node[] valuesInfo)
        {
            int next = valuesInfo[index].next;
            int previous = valuesInfo[index].previous;

            if (next != -1)
                valuesInfo[next].previous = previous;
            if (previous != -1)
                valuesInfo[previous].next = next;
        }

        public struct FasterDictionaryKeyValueEnumerator : IEnumerator<KeyValuePairFast>
        {
            public FasterDictionaryKeyValueEnumerator(SpanDictionary<TKey, TValue> dic) : this()
            {
                _dic = dic;
                _index = -1;
                _count = (int) dic.Count;
            }

            public void Dispose()
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if DEBUG && !PROFILER
                if (_count != _dic.Count)
                    throw new FasterDictionaryException("can't modify a dictionary during its iteration");
#endif
                if (_index < _count - 1)
                {
                    ++_index;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public KeyValuePairFast Current => new KeyValuePairFast(_dic._valuesInfo, _dic._values, _index);

            object IEnumerator.Current => throw new NotImplementedException();

            readonly SpanDictionary<TKey, TValue> _dic;
            readonly int                            _count;

            int _index;
        }

        public struct Node
        {
            public readonly TKey key;
            public readonly int  hashcode;
            public          int  previous;
            public          int  next;

            public Node(ref TKey key, int hash, int previousNode)
            {
                this.key = key;
                hashcode = hash;
                previous = previousNode;
                next = -1;
            }

            public Node(ref TKey key, int hash)
            {
                this.key = key;
                hashcode = hash;
                previous = -1;
                next = -1;
            }
        }

        SveltoMemory<TValue> _values;
        Node[]   _valuesInfo;
        int[]    _buckets;
        uint     _freeValueCellIndex;
        uint     _collisions;

        public struct KeyValuePairFast
        {
            readonly SveltoMemory<TValue> _dicValues;
            readonly Node[]   _dickeys;
            readonly int      _index;

            public KeyValuePairFast(Node[] keys, SveltoMemory<TValue> dicValues, int index)
            {
                _dicValues = dicValues;
                _index = index;
                _dickeys = keys;
            }

            public     TKey   Key   => _dickeys[_index].key;
            public ref TValue Value => ref _dicValues[_index];
        }
    }
}
#endif