using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct NativeGroupsEnumerable<T1, T2, T3>
        where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent where T3 : unmanaged, IEntityComponent
    {
        readonly EntitiesDB       _db;
        readonly ExclusiveGroup[] _groups;

        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroup[] groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroup[] groups) : this()
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
            readonly EntitiesDB                                               _entitiesDB;
        }

        public NativeGroupsIterator GetEnumerator() { return new NativeGroupsIterator(_db, _groups); }
    }

    public struct NativeGroupsEnumerable<T1, T2> where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
    {
        public NativeGroupsEnumerable(EntitiesDB db, ExclusiveGroup[] groups)
        {
            _db = db;
            _groups = groups;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
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

            public void Reset()
            {
                _indexGroup = -1;
            }

            public BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> Current => _array;

            readonly EntitiesDB       _db;
            readonly ExclusiveGroup[] _groups;

            int                                             _indexGroup;
            BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB       _db;
        readonly ExclusiveGroup[] _groups;
    }
}