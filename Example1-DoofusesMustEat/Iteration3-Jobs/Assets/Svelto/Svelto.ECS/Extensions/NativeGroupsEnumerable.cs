using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct NativeGroupsEnumerable<T1, T2, T3>
        where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct where T3 : unmanaged, IEntityStruct
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

    public struct NativeGroupsEnumerable<T1, T2> where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct
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
    
    public struct NativeAllGroupsEnumerable<T1> where T1 : unmanaged, IEntityStruct
    {
        public NativeAllGroupsEnumerable(EntitiesDB db)
        {
            _db = db;
        }

        public struct NativeGroupsIterator
        {
            public NativeGroupsIterator(EntitiesDB db) : this()
            {
                _db = db.FindGroups<T1>().GetEnumerator();
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_db.MoveNext() == true)
                {
                    FasterDictionary<uint, ITypeSafeDictionary>.KeyValuePairFast group = _db.Current;

                    ITypeSafeDictionary<T1> typeSafeDictionary = @group.Value as ITypeSafeDictionary<T1>;
                    
                    if (typeSafeDictionary.Count == 0) continue;
                    
                    _array = new EntityCollection<T1>(typeSafeDictionary.GetValuesArray(out var count), count)
                        .ToNativeBuffer<T1>(out _);

                    return true;
                }

                return false;
            }

            public void Reset()
            {
            }

            public NativeBuffer<T1> Current => _array;

            readonly FasterDictionary<uint, ITypeSafeDictionary>.FasterDictionaryKeyValueEnumerator _db;

            NativeBuffer<T1> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db);
        }

        readonly EntitiesDB       _db;
 }
}