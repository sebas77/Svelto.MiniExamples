#if UNITY_2019_2_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public static class EntityDBExtensions
    {
        public static NativeGroupEnumerable<T1, T2> GroupIterators<T1, T2>(this IEntitiesDB db, ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct
        {
            return new NativeGroupEnumerable<T1, T2>(db, groups);
        }

        public static NativeGroupEnumerable<T1, T2, T3> GroupIterators
            <T1, T2, T3>(this IEntitiesDB db, ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct where T3 : unmanaged, IEntityStruct
        {
            return new NativeGroupEnumerable<T1, T2, T3>(db, groups);
        }

        public static JobHandle CombineDispose
            <T1, T2>(this in BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> buffer, JobHandle combinedDependencies,
                     JobHandle                                               inputDeps)
            where T1 : unmanaged where T2 : unmanaged
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                                                 new DisposeJob<BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>>>(buffer)
                                                    .Schedule(inputDeps));
        }

        public static JobHandle CombineDispose
            <T1, T2, T3>(this in BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>, NativeBuffer<T3>> buffer,
                         JobHandle                                                                 combinedDependencies,
                         JobHandle                                                                 inputDeps)
            where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                                                 new DisposeJob<BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>,
                                                     NativeBuffer<T3>>>(buffer).Schedule(inputDeps));
        }
    }

    public struct NativeGroupEnumerable<T1, T2> where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct
    {
        public NativeGroupEnumerable(IEntitiesDB db, ExclusiveGroup[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupIterator
        {
            public NativeGroupIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;

                    _array = entityCollection.ToNativeBuffers<T1, T2>();
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset() { _indexGroup = -1; }

            public BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> Current => _array;

            readonly IEntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;

            int                                             _indexGroup;
            BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> _array;
        }

        public NativeGroupIterator GetEnumerator() { return new NativeGroupIterator(_db, _groups); }
        
        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;
    }

    public struct NativeGroupEnumerable<T1, T2, T3>
        where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct where T3 : unmanaged, IEntityStruct
    {
        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public NativeGroupEnumerable(IEntitiesDB db, ExclusiveGroup[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupIterator
        {
            public NativeGroupIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.Length)
                {
                    var entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection.count == 0) continue;

                    _array = entityCollection.ToNativeBuffers<T1, T2, T3>();
                    break;
                }

                return _indexGroup < _groups.Length;
            }

            public void Reset() { _indexGroup = -1; }

            public BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>, NativeBuffer<T3>> Current => _array;

            readonly ExclusiveGroup[] _groups;

            int                                                               _indexGroup;
            BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>, NativeBuffer<T3>> _array;
            readonly IEntitiesDB                                              _entitiesDB;
        }

        public NativeGroupIterator GetEnumerator() { return new NativeGroupIterator(_db, _groups); }
    }
}
#endif