using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public ref struct EntityCollection<T, Buffer> where Buffer : struct, IBuffer<T>
    {
        public EntityCollection(T[] array, uint count):this()
        {
            _array.Set(array);
            _count = count;
        }
        
        public EntityCollection(Buffer array, uint count)
        {
            _array = array;
            _count = count;
        }

        public EntityCollection(GCHandle alloc, uint count):this()
        {
            _array.Set(alloc, count);
            _count = count;
        }

        public uint length => _count;
        
        readonly Buffer _array;
        readonly uint   _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToFastAccess(out uint actualCount)
        {
            actualCount = _count;
            return _array.ToManagedArray();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ToNativeFastAccess(out uint actualCount)
        {
            actualCount = _count;
            return _array.ToNativeArray();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Buffer ToBuffer()
        {
            return _array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityCollection<T1, NativeBuffer<T1>> ToNative<T1>() where T1 : unmanaged
        {
            return new EntityCollection<T1, NativeBuffer<T1>>(GCHandle.Alloc(_array.ToManagedArray(),
                                                                             GCHandleType.Pinned), _count);
        }

        public void Dispose() { _array.Dispose(); }
        
        public ref T this[uint i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[i];
        }

        public ref T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[i];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator() { return new EntityIterator(_array, _count); }
        public EntityNativeIterator<NT> GetNativeEnumerator<NT>() where NT : unmanaged, T { return new EntityNativeIterator<NT>(_array, _count); }

        public struct EntityIterator
        {
            public EntityIterator(Buffer array, uint count) : this()
            {
                _array = array.ToManagedArray();
                _count = count;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() { return ++_index < _count; }

            public ref readonly T current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _array[_index];
            }

            readonly T[]  _array;
            readonly uint _count;
            int           _index;
        }
        
        public struct EntityNativeIterator<NT>:IDisposable where NT : unmanaged
        {
            public EntityNativeIterator(Buffer array, uint count) : this()
            {
                unsafe
                {
                    _array = array;
                    _index = (int*) Marshal.AllocHGlobal(sizeof(int));
                    *_index = -1;
                }
            }

            public ref NT threadSafeNext
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    unsafe
                    {
                        return ref ((NT*) _array.ToNativeArray())[Interlocked.Increment(ref *_index)];
                    }
                }
            }

            public ref readonly NT current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    unsafe
                    {
                        return ref ((NT*) _array.ToNativeArray())[*_index];
                    }
                }
            }

            public void Dispose() {
                unsafe
                {
                    _array.Dispose(); Marshal.FreeHGlobal((IntPtr) _index);
                }
            }

            readonly Buffer  _array;
#if ENABLE_BURST_AOT        
            [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif            
            unsafe int *          _index;
        }
    }

    public ref struct EntityCollection<T1, T2, BufferT1, BufferT2>
        where BufferT1 : struct, IBuffer<T1> where BufferT2 : struct, IBuffer<T2>
    {
        public EntityCollection(in EntityCollection<T1, BufferT1> array1, in EntityCollection<T2, BufferT2> array2)
        {
            _array1 = array1;
            _array2 = array2;
        }

        public uint                           length => _array1.length;
        public EntityCollection<T2, BufferT2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        public EntityCollection<T1, BufferT1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        readonly EntityCollection<T1, BufferT1> _array1;
        EntityCollection<T2, BufferT2> _array2;

        public EntityCollection<NT1, NT2, NativeBuffer<NT1>, NativeBuffer<NT2>> ToNative<NT1, NT2>()
            where NT1 : unmanaged, T1 where NT2 : unmanaged, T2
        {
            return new
                EntityCollection<NT1, NT2, NativeBuffer<NT1>, NativeBuffer<NT2>
                >(Item1.ToNative<NT1>(), Item2.ToNative<NT2>());
        }
        
        public (T1[], T2[]) ToFastArray(out uint count)
        {
            count = length;
            
            return (_array1.ToFastAccess(out _), _array2.ToFastAccess(out _));
        }

        public void Dispose()
        {
            _array1.Dispose();
            _array2.Dispose();
        }

        public BufferTuple<BufferT1, BufferT2> ToBuffers()
        {
            var bufferTuple = new BufferTuple<BufferT1, BufferT2>
                (_array1.ToBuffer(), _array2.ToBuffer(), length);
            return bufferTuple;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator() { return new EntityIterator(_array1, _array2); }

        public ref struct EntityIterator
        {
            public EntityIterator(in EntityCollection<T1, BufferT1> array1, in EntityCollection<T2, BufferT2> array2) : this()
            {
                _array1 = array1;
                _array2 = array2;
                _count = array1.length;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() { return ++_index < _count; }

            public void Reset() { _index = -1; }

            public ValueRef<T1, T2, BufferT1, BufferT2> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new ValueRef<T1, T2, BufferT1, BufferT2>(_array1, _array2, (uint) _index);
            }

            public void Dispose() { }

            readonly EntityCollection<T1, BufferT1> _array1;
            readonly EntityCollection<T2, BufferT2> _array2;
            readonly uint                           _count;
            int                                     _index;
        }
    }

    public ref struct EntityCollection<T1, T2, T3, BufferT1, BufferT2, BufferT3>
        where BufferT1 : struct, IBuffer<T1> where BufferT2 : struct, IBuffer<T2> where BufferT3 : struct, IBuffer<T3>
    {
        EntityCollection<T1, BufferT1> _array1;
        EntityCollection<T2, BufferT2> _array2;
        EntityCollection<T3, BufferT3> _array3;

        public EntityCollection(
            in EntityCollection<T1, BufferT1> array1, in EntityCollection<T2, BufferT2> array2, in EntityCollection<T3, BufferT3> array3)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
        }

        public EntityCollection<T1, BufferT1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        public EntityCollection<T2, BufferT2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        public EntityCollection<T3, BufferT3> Item3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }

        public uint length => Item1.length;

        public EntityCollection<NT1, NT2, NT3, NativeBuffer<NT1>, NativeBuffer<NT2>, NativeBuffer<NT3>> ToNative<NT1, NT2, NT3>()
            where NT1 : unmanaged, T1 where NT2 : unmanaged, T2 where NT3 : unmanaged, T3
        {
            return new
                EntityCollection<NT1, NT2, NT3, NativeBuffer<NT1>, NativeBuffer<NT2>, NativeBuffer<NT3>
                >(Item1.ToNative<NT1>(), Item2.ToNative<NT2>(), Item3.ToNative<NT3>());
        }

        public void Dispose()
        {
            _array1.Dispose();
            _array2.Dispose();
            _array3.Dispose();
        }

        public (T1[], T2[], T3[]) ToFastArray(out uint count)
        {
            count = length;
            
            return (_array1.ToFastAccess(out _), _array2.ToFastAccess(out _), _array3.ToFastAccess(out _));
        }
        
        public BufferTuple<BufferT1, BufferT2, BufferT3> ToBuffers()
        {
            var bufferTuple = new BufferTuple<BufferT1, BufferT2, BufferT3>
                (_array1.ToBuffer(), _array2.ToBuffer(), _array3.ToBuffer(), length);
            return bufferTuple;
        }
    }

    public readonly struct BufferTuple<BufferT1, BufferT2, BufferT3>:IDisposable where BufferT1:IDisposable where BufferT2:IDisposable where BufferT3:IDisposable
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly uint length;

        public BufferTuple(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, uint length) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.length = length;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();
        }
    }
    
    public readonly struct BufferTuple<BufferT1, BufferT2>:IDisposable where BufferT1:IDisposable where BufferT2:IDisposable
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly uint     length;

        public BufferTuple(BufferT1 bufferT1, BufferT2 bufferT2, uint length) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.length = length;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
        }
    }

    public struct EntityCollections<T> where T : struct, IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_db, _groups); }

        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array = _db.QueryEntities<T>(_groups[_indexGroup]);
                    _count = _array.length;
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;
                _count      = 0;
            }

            public ref T Current => ref _array[(uint) _index];

            readonly IEntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;

            EntityCollection<T, ManagedBuffer<T>> _array;
            uint                _count;
            int                 _index;
            int                 _indexGroup;
        }
    }

    public struct EntityCollections<T1, T2> where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db     = db;
            _groups = groups;
        }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_db, _groups); }

        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
                _index      = -1;
            }

            public bool MoveNext()
            {
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array1 = _db.QueryEntities<T1>(_groups[_indexGroup]);
                    _array2 = _db.QueryEntities<T2>(_groups[_indexGroup]);
                    _count = _array1.length;

#if DEBUG && !PROFILER
                    if (_count != _array2.Length)
                        throw new ECSException("number of entities in group doesn't match");
#endif
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index      = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1>(_groups[0]);
                _array2 = _db.QueryEntities<T2>(_groups[0]);
                _count = _array1.length;
#if DEBUG && !PROFILER
                if (_count != _array2.Length)
                    throw new ECSException("number of entities in group doesn't match");
#endif
            }

            public ValueRef<T1, T2, ManagedBuffer<T1>, ManagedBuffer<T2>> Current
            {
                get
                {
                    var valueRef = new ValueRef<T1, T2, ManagedBuffer<T1>, ManagedBuffer<T2>>(_array1, _array2, (uint) _index);
                    return valueRef;
                }
            }

            readonly IEntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;
            uint                      _count;
            int                       _index;
            int                       _indexGroup;

            EntityCollection<T1, ManagedBuffer<T1>> _array1; 
            EntityCollection<T2, ManagedBuffer<T2>> _array2;
        }
    }

    public ref struct ValueRef<T1, T2, BufferT1, BufferT2>
        where BufferT1 : struct, IBuffer<T1> where BufferT2 : struct, IBuffer<T2>
    {
        readonly EntityCollection<T1, BufferT1> array1; 
        readonly EntityCollection<T2, BufferT2> array2;

        readonly uint index;

        public ValueRef(
            in EntityCollection<T1, BufferT1> entity1, in EntityCollection<T2, BufferT2> entity2,
            uint                              i)
        {
            array1 = entity1;
            array2 = entity2;
            index  = i;
        }

        public ref T1 entityStructA
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1[index];
        }

        public ref T2 entityStructB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array2[index];
        }
    }

    public ref struct ValueRef<T1, T2, T3, BufferT1, BufferT2, BufferT3>
        where BufferT1 : struct, IBuffer<T1> where BufferT2 : struct, IBuffer<T2> where BufferT3 : struct, IBuffer<T3>
    {
        readonly EntityCollection<T1, BufferT1> array1; 
        readonly EntityCollection<T2, BufferT2> array2;
        readonly EntityCollection<T3, BufferT3> array3;

        readonly uint index;

        public ValueRef(
            in EntityCollection<T1, BufferT1> entity1, in EntityCollection<T2, BufferT2> entity2, in EntityCollection<T3, BufferT3> entity3,
            uint                                                                                                i)
        {
            array1 = entity1;
            array2 = entity2;
            array3 = entity3;
            index = i;
        }

        public ref T1 entityStructA
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1[index];
        }

        public ref T2 entityStructB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array2[index];
        }

        public ref T3 entityStructC
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array3[index];
        }
    }
}