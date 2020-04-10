using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct NativeAllGroupsEnumerable<T1> where T1 : unmanaged, IEntityComponent
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
                        .ToNativeBuffer<T1>();

                    return true;
                }

                return false;
            }

            public void Reset()
            {
            }

            public NB<T1> Current => _array;

            readonly FasterDictionary<uint, ITypeSafeDictionary>.FasterDictionaryKeyValueEnumerator _db;

            NB<T1> _array;
        }

        public NativeGroupsIterator GetEnumerator()
        {
            return new NativeGroupsIterator(_db);
        }

        readonly EntitiesDB       _db;
    }
}