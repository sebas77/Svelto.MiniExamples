using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Utilities;

namespace Svelto.DataStructures
{
    sealed class SveltoDictionaryDebugProxy<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy>
        where TKey : struct, IEquatable<TKey>
        where TKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
        where TValueStrategy : struct, IBufferStrategy<TValue>
        where TBucketStrategy : struct, IBufferStrategy<int>
    {
        public SveltoDictionaryDebugProxy
            (SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> dic)
        {
            _dic = dic;
        }

        public uint count => (uint)_dic.count;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePairFast<TKey, TValue, TValueStrategy>[] keyValues
        {
            get
            {
                var array = new KeyValuePairFast<TKey, TValue, TValueStrategy>[_dic.count];

                int i = 0;
                foreach (var keyValue in _dic)
                {
                    array[i++] = keyValue;
                }

                return array;
            }
        }

        readonly SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> _dic;
    }

    /// <summary>
    /// This dictionary has been created for just one reason: I needed a dictionary that would have let me iterate
    /// over the values as an array, directly, without generating one or using an iterator.
    /// For this goal is N times faster than the standard dictionary. Faster dictionary is also faster than
    /// the standard dictionary for most of the operations, but the difference is negligible. The only slower operation
    /// is resizing the memory on add, as this implementation needs to use two separate arrays compared to the standard
    /// one
    /// note: SveltoDictionary is not thread safe. A thread safe version should take care of possible setting of
    /// value with shared hash hence bucket list index.
    /// </summary>
    [DebuggerTypeProxy(typeof(SveltoDictionaryDebugProxy<,,,,>))]
    public struct SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> : IDisposable
        where TKey : struct, IEquatable<TKey>
        where TKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
        where TValueStrategy : struct, IBufferStrategy<TValue>
        where TBucketStrategy : struct, IBufferStrategy<int>
    {
        static SveltoDictionary()
        {
            try
            {
                if (typeof(TKey).GetMethod("GetHashCode"
                                         , BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                 == null)
                    Svelto.Console.LogWarning(typeof(TKey).Name
                                            + " does not implement GetHashCode -> This will cause unwanted allocations (boxing)");
            }
            catch (AmbiguousMatchException) { }
        }

        public SveltoDictionary(uint size, Allocator allocator) : this()
        {
            //AllocationStrategy must be passed external for TValue because SveltoDictionary doesn't have struct
            //constraint needed for the NativeVersion
            _valuesInfo = default;
            _valuesInfo.Alloc(size, allocator);
            _values = default;
            _values.Alloc(size, allocator);
            _buckets = default;
            _buckets.Alloc((uint)HashHelpers.GetPrime((int)size), allocator);

            if (size > 0)
                _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier(size);
        }

        public TKeyStrategy unsafeKeys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _valuesInfo;
        }

        public TValueStrategy unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values;
        }

        /// <summary>
        /// Note: the NativeStrategy implementations always hold an pre-boxed version of the buffer, so boxing
        /// never happens at run time. Unboxing does happen at runtime, but it's very cheap and never incur in
        /// allocations 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> UnsafeGetValues(out uint count)
        {
            count = _freeValueCellIndex;

            return _values.ToBuffer();
        }

        public int  count   => (int)_freeValueCellIndex;
        public bool isValid => _buckets.isValid;

        public SveltoDictionaryKeyEnumerable keys => new SveltoDictionaryKeyEnumerable(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //note, this returns readonly because the enumerator cannot be, but at the same time, it cannot be modified
        public SveltoDictionaryKeyValueEnumerator<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy>
            GetEnumerator()
        {
            return new SveltoDictionaryKeyValueEnumerator<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy>(
                this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, in TValue value)
        {
            var ret = AddValue(key, out var index);

#if DEBUG && !PROFILE_SVELTO
            if (ret == false)
                throw new SveltoDictionaryException("Key already present");
#endif

            _values[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, in TValue value, out uint index)
        {
            var ret = AddValue(key, out index);

            if (ret == true)
                _values[index] = value;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TKey key, in TValue value)
        {
            var ret = AddValue(key, out var index);

#if DEBUG && !PROFILE_SVELTO
            if (ret == true)
                throw new SveltoDictionaryException("trying to set a value on a not existing key");
#endif

            _values[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            //Buckets cannot be FastCleared because it's important that the values are reset to 0
            _buckets.Clear();

            if (IsUnmanaged() == false)
            {
                _values.Clear();
                _valuesInfo.Clear();
            }
        }

        static bool IsUnmanaged()
        {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.IsUnmanaged<TValue>();
#else
            return typeof(TValue).IsUnmanagedEx();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            if (_freeValueCellIndex == 0)
                return;

            _freeValueCellIndex = 0;

            //Buckets cannot be FastCleared because it's important that the values are reset to 0
            _buckets.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read 
        //constant states) because it will be used in multithreaded parallel code
        public bool ContainsKey(TKey key)
        {
            return TryFindIndex(key, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read 
        //constant states) because it will be used in multithreaded parallel code
        public bool TryGetValue(TKey key, out TValue result)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                result = _values[(int)findIndex];
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(TKey key)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int)findIndex];
            }

            AddValue(key, out findIndex);

            _values[(int)findIndex] = default;

            return ref _values[(int)findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(TKey key, Func<TValue> builder)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int)findIndex];
            }

            AddValue(key, out findIndex);

            _values[(int)findIndex] = builder();

            return ref _values[(int)findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd<W>(TKey key, FuncRef<W, TValue> builder, ref W parameter)
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int)findIndex];
            }

            AddValue(key, out findIndex);

            _values[(int)findIndex] = builder(ref parameter);

            return ref _values[(int)findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue RecycleOrAdd<TValueProxy>
            (TKey key, Func<TValueProxy> builder, ActionRef<TValueProxy> recycler) where TValueProxy : class, TValue
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int)findIndex];
            }

            AddValue(key, out findIndex);

            if (_values[(int)findIndex] == null)
                _values[(int)findIndex] = builder();
            else
                recycler(ref Unsafe.As<TValue, TValueProxy>(ref _values[(int)findIndex]));

            return ref _values[(int)findIndex];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on dictionaries that are fast cleared and use objects
        /// as value. Once the dictionary is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue RecycleOrAdd<TValueProxy, W>
            (TKey key, FuncRef<W, TValue> builder, ActionRef<TValueProxy, W> recycler, ref W parameter)
            where TValueProxy : class, TValue
        {
            if (TryFindIndex(key, out var findIndex) == true)
            {
                return ref _values[(int)findIndex];
            }

            AddValue(key, out findIndex);

            if (_values[(int)findIndex] == null)
                _values[(int)findIndex] = builder(ref parameter);
            else
                recycler(ref Unsafe.As<TValue, TValueProxy>(ref _values[(int)findIndex]), ref parameter);

            return ref _values[(int)findIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read 
        //constant states) because it will be used in multi-threaded parallel code
        public ref TValue GetDirectValueByRef(uint index)
        {
            return ref _values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(TKey key)
        {
#if DEBUG && !PROFILE_SVELTO
            if (TryFindIndex(key, out var findIndex) == true)
                return ref _values[(int)findIndex];

            throw new SveltoDictionaryException("Key not found");
#else
            //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            TryFindIndex(key, out var findIndex);

            return ref _values[(int) findIndex];
#endif
        }

        public void EnsureCapacity(uint size)
        {
            if (_values.capacity < size)
            {
                ResizeStorage(size);
            }
        }

        public void IncreaseCapacityBy(uint size)
        {
            ResizeStorage((uint)(_values.capacity + (int)size));
        }
        
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[(int)GetIndex(key)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                AddValue(key, out var index);

                _values[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AddValue(TKey key, out uint indexSet)
        {
            int  hash        = key.GetHashCode(); //IEquatable doesn't enforce the override of GetHashCode
            uint bucketIndex = Reduce((uint)hash, (uint)_buckets.capacity, _fastModBucketsMultiplier);

            //buckets value -1 means it's empty
            var valueIndex = _buckets[bucketIndex] - 1;

            if (valueIndex == -1)
            {
                ResizeIfNeeded();
                //create the info node at the last position and fill it with the relevant information
                _valuesInfo[_freeValueCellIndex] = new SveltoDictionaryNode<TKey>(ref key, hash);
            }
            else //collision or already exists
            {
                int currentValueIndex = valueIndex;
                do
                {
                    //must check if the key already exists in the dictionary
                    //Comparer<TKey>.default needs to create a new comparer, so it is much slower
                    //than assuming that Equals is implemented through IEquatable (but what if the comparer is statically cached?)
                    ref var fasterDictionaryNode = ref _valuesInfo[currentValueIndex];
                    if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                    {
                        //the key already exists, simply replace the value!
                        indexSet = (uint)currentValueIndex;
                        return false;
                    }

                    currentValueIndex = fasterDictionaryNode.previous;
                } while (currentValueIndex != -1); //-1 means no more values with key with the same hash

                ResizeIfNeeded();

                //oops collision!
                _collisions++;
                //create a new node which previous index points to node currently pointed in the bucket
                _valuesInfo[_freeValueCellIndex] = new SveltoDictionaryNode<TKey>(ref key, hash, valueIndex);
                //update the next of the existing cell to point to the new one
                //old one -> new one | old one <- next one
                _valuesInfo[valueIndex].next = (int)_freeValueCellIndex;
                //Important: the new node is always the one that will be pointed by the bucket cell
                //so I can assume that the one pointed by the bucket is always the last value added
                //(next = -1)
            }

            //item with this bucketIndex will point to the last value created
            //ToDo: if instead I assume that the original one is the one in the bucket
            //I wouldn't need to update the bucket here. Small optimization but important
            _buckets[bucketIndex] = (int)(_freeValueCellIndex + 1);

            indexSet = _freeValueCellIndex;
            _freeValueCellIndex++;

            //too many collisions?
            if (_collisions > _buckets.capacity)
                ResizeBucket(_collisions);

            return true;
        }

        void ResizeBucket(uint newSize)
        {
            //we need more space and less collisions
            _buckets.Resize((uint)HashHelpers.Expand((int)newSize), false, true);
            _collisions               = 0;
            _fastModBucketsMultiplier = HashHelpers.GetFastModMultiplier((uint)_buckets.capacity);
            var bucketsCapacity = (uint)_buckets.capacity;

            //we need to get all the hash code of all the values stored so far and spread them over the new bucket
            //length
            var freeValueCellIndex = _freeValueCellIndex;
            for (int newValueIndex = 0; newValueIndex < freeValueCellIndex; ++newValueIndex)
            {
                //get the original hash code and find the new bucketIndex due to the new length
                ref var valueInfoNode   = ref _valuesInfo[newValueIndex];
                var    bucketIndex = Reduce((uint)valueInfoNode.hashcode, bucketsCapacity, _fastModBucketsMultiplier);
                //bucketsIndex can be -1 or a next value. If it's -1 means no collisions. If there is collision,
                //we create a new node which prev points to the old one. Old one next points to the new one.
                //the bucket will now points to the new one
                //In this way we can rebuild the linkedlist.
                //get the current valueIndex, it's -1 if no collision happens
                int existingValueIndex = _buckets[bucketIndex] - 1;
                //update the bucket index to the index of the current item that share the bucketIndex
                //(last found is always the one in the bucket)
                _buckets[bucketIndex] = newValueIndex + 1;
                if (existingValueIndex == -1)
                {
                    //ok nothing was indexed, the bucket was empty. We need to update the previous
                    //values of next and previous
                    valueInfoNode.next     = -1;
                    valueInfoNode.previous = -1;
                }
                else
                {
                    //oops a value was already being pointed by this cell in the new bucket list,
                    //it means there is a collision, problem
                    _collisions++;
                    //the bucket will point to this value, so 
                    //the previous index will be used as previous for the new value.
                    valueInfoNode.previous = existingValueIndex;
                    valueInfoNode.next     = -1;
                    //and update the previous next index to the new one
                    _valuesInfo[existingValueIndex].next = newValueIndex;
                }
            }
        }

        public bool Remove(TKey key)
        {
            return Remove(key, out _, out _);
        }

        public bool Remove(TKey key, out int index, out TValue value)
        {
            int  hash        = key.GetHashCode();
            uint bucketIndex = Reduce((uint)hash, (uint)_buckets.capacity, _fastModBucketsMultiplier);

            //find the bucket
            int indexToValueToRemove = _buckets[bucketIndex] - 1;

            //Part one: look for the actual key in the bucket list if found I update the bucket list so that it doesn't
            //point anymore to the cell to remove
            while (indexToValueToRemove != -1)
            {
                ref var fasterDictionaryNode = ref _valuesInfo[indexToValueToRemove];
                if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                {
                    //if the key is found and the bucket points directly to the node to remove
                    if (_buckets[bucketIndex] - 1 == indexToValueToRemove)
                    {
#if DEBUG && !PROFILE_SVELTO
                        if (fasterDictionaryNode.next != -1)
                            throw new SveltoDictionaryException(
                                "if the bucket points to the cell, next MUST NOT exists");
#endif
                        //the bucket will point to the previous cell. if a previous cell exists
                        //its next pointer must be updated!
                        //<--- iteration order  
                        //                      Bucket points always to the last one
                        //   ------- ------- -------
                        //   |  1  | |  2  | |  3  | //bucket cannot have next, only previous
                        //   ------- ------- -------
                        //--> insert order
                        _buckets[bucketIndex] = fasterDictionaryNode.previous + 1;
                    }
#if DEBUG && !PROFILE_SVELTO
                    else
                    {
                        if (fasterDictionaryNode.next == -1)
                            throw new SveltoDictionaryException(
                                "if the bucket points to another cell, next MUST exists");
                    }
#endif

                    UpdateLinkedList(indexToValueToRemove, ref _valuesInfo);

                    break;
                }

                indexToValueToRemove = fasterDictionaryNode.previous;
            }

            if (indexToValueToRemove == -1)
            {
                index = default;
                value = default;
                return false; //not found!
            }

            index = indexToValueToRemove;

            _freeValueCellIndex--; //one less value to iterate
            value = _values[indexToValueToRemove];

            //Part two:
            //At this point nodes pointers and buckets are updated, but the _values array
            //still has got the value to delete. Remember the goal of this dictionary is to be able
            //to iterate over the values like an array, so the values array must always be up to date

            //if the cell to remove is the last one in the list, we can perform less operations (no swapping needed)
            //otherwise we want to move the last value cell over the value to remove
            if (indexToValueToRemove != _freeValueCellIndex)
            {
                //we can move the last value of both arrays in place of the one to delete.
                //in order to do so, we need to be sure that the bucket pointer is updated.
                //first we find the index in the bucket list of the pointer that points to the cell
                //to move
                ref var fasterDictionaryNode = ref _valuesInfo[_freeValueCellIndex];
                var movingBucketIndex = Reduce((uint)fasterDictionaryNode.hashcode, (uint)_buckets.capacity
                                             , _fastModBucketsMultiplier);

                //if the key is found and the bucket points directly to the node to remove
                //it must now point to the cell where it's going to be moved
                if (_buckets[movingBucketIndex] - 1 == _freeValueCellIndex)
                    _buckets[movingBucketIndex] = indexToValueToRemove + 1;

                //otherwise it means that there was more than one key with the same hash (collision), so 
                //we need to update the linked list and its pointers
                int next     = fasterDictionaryNode.next;
                int previous = fasterDictionaryNode.previous;

                //they now point to the cell where the last value is moved into
                if (next != -1)
                    _valuesInfo[next].previous = indexToValueToRemove;
                if (previous != -1)
                    _valuesInfo[previous].next = indexToValueToRemove;

                //finally, actually move the values
                _valuesInfo[indexToValueToRemove] = fasterDictionaryNode;
                _values[indexToValueToRemove]     = _values[_freeValueCellIndex];
            }

            return true;
        }
        
        public void Trim()
        {
            _values.Resize(_freeValueCellIndex);
            _valuesInfo.Resize(_freeValueCellIndex);
        }

        //I store all the index with an offset + 1, so that in the bucket list 0 means actually not existing.
        //When read the offset must be offset by -1 again to be the real one. In this way
        //I avoid to initialize the array to -1

        //WARNING this method must stay stateless (not relying on states that can change, it's ok to read 
        //constant states) because it will be used in multithreaded parallel code
        public bool TryFindIndex(TKey key, out uint findIndex)
        {
            DBC.Common.Check.Require(_buckets.capacity > 0, "Dictionary arrays are not correctly initialized (0 size)");

            int hash = key.GetHashCode();

            uint bucketIndex = Reduce((uint)hash, (uint)_buckets.capacity, _fastModBucketsMultiplier);

            int valueIndex = _buckets[bucketIndex] - 1;

            //even if we found an existing value we need to be sure it's the one we requested
            while (valueIndex != -1)
            {
                //Comparer<TKey>.default needs to create a new comparer, so it is much slower
                //than assuming that Equals is implemented through IEquatable
                ref var fasterDictionaryNode = ref _valuesInfo[valueIndex];
                if (fasterDictionaryNode.hashcode == hash && fasterDictionaryNode.key.Equals(key) == true)
                {
                    //this is the one
                    findIndex = (uint)valueIndex;
                    return true;
                }

                valueIndex = fasterDictionaryNode.previous;
            }

            findIndex = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(TKey key)
        {
#if DEBUG && !PROFILE_SVELTO
            if (TryFindIndex(key, out var findIndex) == true)
                return findIndex;

            throw new SveltoDictionaryException("Key not found");
#else
            //Burst is not able to vectorise code if throw is found, regardless if it's actually ever thrown
            TryFindIndex(key, out var findIndex);
            
            return findIndex;
#endif
        }

        public void Intersect<OTValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy>
            (SveltoDictionary<TKey, OTValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy> otherDicKeys)
            where OTKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
            where OTValueStrategy : struct, IBufferStrategy<OTValue>
            where OTBucketStrategy : struct, IBufferStrategy<int>
        {
            for (int i = count - 1; i >= 0; i--)
            {
                var tKey = unsafeKeys[i].key;
                if (otherDicKeys.ContainsKey(tKey) == false)
                {
                    this.Remove(tKey);
                }
            }
        }

        public void Exclude<OTValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy>
            (SveltoDictionary<TKey, OTValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy> otherDicKeys)
            where OTKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
            where OTValueStrategy : struct, IBufferStrategy<OTValue>
            where OTBucketStrategy : struct, IBufferStrategy<int>
        {
            for (int i = count - 1; i >= 0; i--)
            {
                var tKey = unsafeKeys[i].key;
                if (otherDicKeys.ContainsKey(tKey) == true)
                {
                    this.Remove(tKey);
                }
            }
        }

        public void Union<OTKeyStrategy, OTValueStrategy, OTBucketStrategy>
            (SveltoDictionary<TKey, TValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy> otherDicKeys)
            where OTKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
            where OTValueStrategy : struct, IBufferStrategy<TValue>
            where OTBucketStrategy : struct, IBufferStrategy<int>
        {
            foreach (var other in otherDicKeys)
            {
                this[other.key] = other.value;
            }
        }

        public void CopyFrom<OTKeyStrategy, OTValueStrategy, OTBucketStrategy>
            (SveltoDictionary<TKey, TValue, OTKeyStrategy, OTValueStrategy, OTBucketStrategy> otherDicKeys)
            where OTKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
            where OTValueStrategy : struct, IBufferStrategy<TValue>
            where OTBucketStrategy : struct, IBufferStrategy<int>
        {
            _valuesInfo.SerialiseFrom(otherDicKeys._valuesInfo.AsBytesPointer());
            _values.SerialiseFrom(otherDicKeys._values.AsBytesPointer());
            _buckets.SerialiseFrom(otherDicKeys._buckets.AsBytesPointer());

            this._collisions         = otherDicKeys._collisions;
            this._freeValueCellIndex = otherDicKeys._freeValueCellIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResizeIfNeeded()
        {
            if (_freeValueCellIndex == _values.capacity)
            {
                var expandPrime = HashHelpers.Expand((int)_freeValueCellIndex);

                _values.Resize((uint)expandPrime, true, false);
                _valuesInfo.Resize((uint)expandPrime, true, false);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResizeStorage(uint size)
        {
            var expandPrime = HashHelpers.Expand((int)size);

            _values.Resize((uint)expandPrime, true, false);
            _valuesInfo.Resize((uint)expandPrime, true, false);
            ResizeBucket(size);
        }

        static readonly bool Is64BitProcess = Environment.Is64BitProcess; 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Reduce(uint hashcode, uint N, ulong fastModBucketsMultiplier)
        {
            if (hashcode >= N) //is the condition return actually an optimization?
                return Is64BitProcess
                    ? HashHelpers.FastMod(hashcode, N, fastModBucketsMultiplier)
                    : hashcode % N;

            return hashcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void UpdateLinkedList(int index, ref TKeyStrategy valuesInfo)
        {
            int next     = valuesInfo[index].next;
            int previous = valuesInfo[index].previous;

            if (next != -1)
                valuesInfo[next].previous = previous;
            if (previous != -1)
                valuesInfo[previous].next = next;
        }

        public readonly struct SveltoDictionaryKeyEnumerable
        {
            readonly SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> _dic;

            public SveltoDictionaryKeyEnumerable
                (SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> dic)
            {
                _dic = dic;
            }

            public SveltoDictionaryKeyEnumerator GetEnumerator() => new SveltoDictionaryKeyEnumerator(_dic);
        }

        public struct SveltoDictionaryKeyEnumerator
        {
            public SveltoDictionaryKeyEnumerator
                (SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> dic) : this()
            {
                _dic   = dic;
                _index = -1;
                _count = dic.count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
#if DEBUG && !PROFILE_SVELTO
                if (_count != _dic.count)
                    throw new SveltoDictionaryException("can't modify a dictionary during its iteration");
#endif
                if (_index < _count - 1)
                {
                    ++_index;
                    return true;
                }

                return false;
            }

            public TKey Current => _dic._valuesInfo[_index].key;

            readonly SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> _dic;
            readonly int                                                                           _count;

            int _index;
        }

        public void Dispose()
        {
            _valuesInfo.Dispose();
            _values.Dispose();
            _buckets.Dispose();
        }

        internal TKeyStrategy   _valuesInfo;
        internal TValueStrategy _values;
        TBucketStrategy         _buckets;

        uint  _freeValueCellIndex;
        uint  _collisions;
        ulong _fastModBucketsMultiplier;
    }

    public class SveltoDictionaryException : Exception
    {
        public SveltoDictionaryException(string keyAlreadyExisting) : base(keyAlreadyExisting) { }
    }

    public struct SveltoDictionaryKeyValueEnumerator<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy>
        where TKey : struct, IEquatable<TKey>
        where TKeyStrategy : struct, IBufferStrategy<SveltoDictionaryNode<TKey>>
        where TValueStrategy : struct, IBufferStrategy<TValue>
        where TBucketStrategy : struct, IBufferStrategy<int>
    {
        public SveltoDictionaryKeyValueEnumerator
            (SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> dic) : this()
        {
            _dic   = dic;
            _index = -1;
            _count = dic.count;
#if DEBUG && !PROFILE_SVELTO
            _startCount = dic.count;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
#if DEBUG && !PROFILE_SVELTO
            if (_count != _startCount)
                throw new SveltoDictionaryException("can't modify a dictionary while it is iterated");
#endif
            if (_index < _count - 1)
            {
                ++_index;
                return true;
            }

            return false;
        }

        public KeyValuePairFast<TKey, TValue, TValueStrategy> Current =>
            new KeyValuePairFast<TKey, TValue, TValueStrategy>(_dic._valuesInfo[_index].key, _dic._values, _index);

        SveltoDictionary<TKey, TValue, TKeyStrategy, TValueStrategy, TBucketStrategy> _dic;
#if DEBUG && !PROFILE_SVELTO
        int _startCount;
#endif
        int _count;

        int _index;

        public void SetRange(uint startIndex, uint count)
        {
            _index = (int)startIndex - 1;
            _count = (int)count;
#if DEBUG && !PROFILE_SVELTO
            if (_count > _startCount)
                throw new SveltoDictionaryException("can't set a count greater than the starting one");
            _startCount = (int)count;
#endif
        }
    }

    /// <summary>
    ///the mechanism to use arrays is fundamental to work 
    /// </summary>
    [DebuggerDisplay("[{key}] - {value}")]
    [DebuggerTypeProxy(typeof(KeyValuePairFastDebugProxy<,,>))]
    public readonly struct KeyValuePairFast<TKey, TValue, TValueStrategy> where TKey : struct, IEquatable<TKey>
                                                                          where TValueStrategy : struct,
                                                                          IBufferStrategy<TValue>
    {
        public KeyValuePairFast(TKey keys, TValueStrategy dicValues, int index)
        {
            _dicValues = dicValues;
            _index     = index;
            _key       = keys;
        }

        public     TKey   key   => _key;
        public ref TValue value => ref _dicValues[_index];

        readonly TValueStrategy _dicValues;
        readonly TKey           _key;
        readonly int            _index;
    }

    public sealed class KeyValuePairFastDebugProxy<TKey, TValue, TValueStrategy> where TKey : struct, IEquatable<TKey>
        where TValueStrategy : struct, IBufferStrategy<TValue>
    {
        public KeyValuePairFastDebugProxy(KeyValuePairFast<TKey, TValue, TValueStrategy> keyValue)
        {
            this._keyValue = keyValue;
        }

        public TKey   key   => _keyValue.key;
        public TValue value => _keyValue.value;

        readonly KeyValuePairFast<TKey, TValue, TValueStrategy> _keyValue;
    }
}