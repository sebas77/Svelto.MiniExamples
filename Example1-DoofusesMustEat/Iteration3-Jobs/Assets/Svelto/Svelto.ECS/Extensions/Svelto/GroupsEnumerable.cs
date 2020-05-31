using DBC.ECS;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly struct GroupsEnumerable<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                            where T2 : struct, IEntityComponent
                                                            where T3 : struct, IEntityComponent
                                                            where T4 : struct, IEntityComponent
    {
        readonly EntitiesDB                       _db;
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public GroupsEnumerable(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection1 = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection1.count == 0)
                        continue;
                    var entityCollection2 = _entitiesDB.QueryEntities<T4>(_groups[_indexGroup]);
                    if (entityCollection2.count == 0)
                        continue;

                    Check.Assert(entityCollection1.count == entityCollection2.count
                               , "congratulation, you found a bug in Svelto, please report it");

                    BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>> array  = entityCollection1.ToBuffers();
                    IBuffer<T4> array2 = entityCollection2.ToBuffer();
                    _buffers = new BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>, IBuffer<T4>>(
                        array.buffer1, array.buffer2, array.buffer3, array2, entityCollection1.count);
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>, IBuffer<T4>> Current => _buffers;

            readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                                                    _indexGroup;
            BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>, IBuffer<T4>> _buffers;
            readonly EntitiesDB                                    _entitiesDB;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }
    }

    public readonly struct GroupsEnumerable<T1, T2, T3> where T1 : struct, IEntityComponent
                                                        where T2 : struct, IEntityComponent
                                                        where T3 : struct, IEntityComponent
    {
        readonly EntitiesDB                       _db;
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

        public GroupsEnumerable(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _groups     = groups;
                _indexGroup = -1;
                _entitiesDB = db;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection = _entitiesDB.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection.ToBuffers();
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>> Current => _buffers;

            readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                                       _indexGroup;
            BT<IBuffer<T1>, IBuffer<T2>, IBuffer<T3>> _buffers;
            readonly EntitiesDB                       _entitiesDB;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }
    }

    public readonly struct GroupsEnumerable<T1, T2> where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffers = entityCollection.ToBuffers();
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            //todo move to tuple and add group to as it may be necessary?
            public BT<IBuffer<T1>, IBuffer<T2>> Current => _buffers;

            readonly EntitiesDB                       _db;
            readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int                _indexGroup;
            BT<IBuffer<T1>, IBuffer<T2>> _buffers;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                       _db;
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;
    }

    public readonly struct GroupsEnumerable<T1> where T1 : struct, IEntityComponent
    {
        public GroupsEnumerable(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            _db     = db;
            _groups = groups;
        }

        public struct GroupsIterator
        {
            public GroupsIterator(EntitiesDB db, FasterReadOnlyList<ExclusiveGroupStruct> groups) : this()
            {
                _db         = db;
                _groups     = groups;
                _indexGroup = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (++_indexGroup < _groups.count)
                {
                    var entityCollection = _db.QueryEntities<T1>(_groups[_indexGroup]);
                    if (entityCollection.count == 0)
                        continue;

                    _buffer = new BT<IBuffer<T1>>(entityCollection.ToBuffer(), entityCollection.count);
                    break;
                }

                return _indexGroup < _groups.count;
            }

            public void Reset() { _indexGroup = -1; }

            public BT<IBuffer<T1>> Current => _buffer;

            readonly EntitiesDB                       _db;
            readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;

            int    _indexGroup;
            BT<IBuffer<T1>> _buffer;
        }

        public GroupsIterator GetEnumerator() { return new GroupsIterator(_db, _groups); }

        readonly EntitiesDB                       _db;
        readonly FasterReadOnlyList<ExclusiveGroupStruct> _groups;
    }
}