using System.Collections;
using System.Collections.Generic;

namespace Svelto.ECS
{
    public interface IEntitiesDB: IObsoleteInterfaceDb
    {
        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// return the buffer and the index of the entity inside the buffer using the input EGID
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryQueryEntitiesAndIndex<T>(uint id, uint group, out uint index, out T[] array) where T : IEntityStruct;
        bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint                            index, out T[]  array) where T : IEntityStruct;
        bool TryQueryEntitiesAndIndex<T>(uint id,        ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array) where T : IEntityStruct;

        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// return the buffer and the index of the entity inside the buffer using the input EGID
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntitiesAndIndex<T>(EGID entityGid, out uint index) where T : IEntityStruct;
        T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index) where T : IEntityStruct;
        T[] QueryEntitiesAndIndex<T>(uint id, uint                                group, out uint index) where T : IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="group"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct;
        ref T QueryUniqueEntity<T>(uint group) where T : IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="entityGid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ref T QueryEntity<T>(EGID entityGid) where T : IEntityStruct;
        ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct;
        ref T QueryEntity<T>(uint id, uint                                group) where T : IEntityStruct;

        /// <summary>
        /// Fast and raw (therefore not safe) return of entities buffer
        /// Modifying a buffer would compromise the integrity of the whole DB
        /// so they are meant to be used only in performance critical path
        /// </summary>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] QueryEntities<T>(uint group, out uint count) where T : IEntityStruct;
        T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count) where T : IEntityStruct;
        
        EntityCollection<T>  QueryEntities<T>(uint                                group) where T : IEntityStruct;
        EntityCollection<T>  QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : IEntityStruct;
        EntityCollections<T> QueryEntities<T>(ExclusiveGroup[]                    groups) where T : IEntityStruct;

        (T1[], T2[]) QueryEntities<T1, T2>(uint group, out uint count) where T1 : IEntityStruct where T2 : IEntityStruct;
        (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct;

        (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(uint group, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct where T3 : IEntityStruct;
        (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct where T3 : IEntityStruct;

        /// <summary>
        /// this version returns a mapped version of the entity array so that is possible to find the
        /// index of the entity inside the returned buffer through it's EGID
        /// However mapping can be slow so it must be used for not performance critical paths
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="mapper"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EGIDMapper<T> QueryMappedEntities<T>(uint groupID) where T : IEntityStruct;
        EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId) where T : IEntityStruct;
        /// <summary>
        /// Execute an action on ALL the entities regardless the group. This function doesn't guarantee cache
        /// friendliness even if just EntityStructs are used.
        /// Safety checks are in place
        /// </summary>
        /// <param name="damageableGroups"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        void ExecuteOnAllEntities<T>(System.Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB> action)
            where T : IEntityStruct;
        void ExecuteOnAllEntities<T, W>(ref W                                                                         value,
                                        System.Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB, W> action)
            where T : IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Exists<T>(EGID egid) where T : IEntityStruct;
        bool Exists<T>(uint                              id, uint groupid) where T : IEntityStruct;
        bool Exists (ExclusiveGroup.ExclusiveGroupStruct gid);

        /// <summary>
        ///
        /// </summary>
        /// <param name="group"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasAny<T>(uint group) where T:IEntityStruct;
        bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T:IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="groupStruct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)  where T:IEntityStruct;
        uint Count<T>(uint groupStruct)  where T:IEntityStruct;

        /// <summary>
        ///
        /// </summary>
        /// <param name="egid"></param>
        /// <typeparam name="T"></typeparam>
        void PublishEntityChange<T>(EGID egid)  where T : unmanaged, IEntityStruct;
    }

    public struct EntityCollection<T>: IEnumerable<T>
    {
        public EntityCollection(T[] array, uint count)
        {
            _array = array;
            _count = count;
        }
        IEnumerator IEnumerable.GetEnumerator() =>  throw new System.NotImplementedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>  throw new System.NotImplementedException();
        
        public EntityIterator<T> GetEnumerator() { return new EntityIterator<T>(_array, _count); }
        
        readonly T[]  _array;
        readonly uint _count;
    }
    
    public struct EntityCollections<T>: IEnumerable<T> where T : IEntityStruct
    {
        public EntityCollections(IEntitiesDB db, ExclusiveGroup[] groups):this() { _db = db;
            _groups = groups;
        }
        
        IEnumerator IEnumerable.GetEnumerator() =>  throw new System.NotImplementedException();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>  throw new System.NotImplementedException();
        
        public EntityGroupsIterator<T> GetEnumerator() { return new EntityGroupsIterator<T>(_db, _groups); }

        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;
    }
    
    public struct EntityGroupsIterator<T>:IEnumerator<T> where T : IEntityStruct
    {
        public EntityGroupsIterator(IEntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db     = db;
            _groups = groups;
            _indexGroup = -1;
            _index = -1;
        }

        public bool MoveNext()
        {
            while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
            {
                _index = -1;
                _array = _db.QueryEntities<T>(_groups[_indexGroup], out _count);
            }
            
            if (++_index < _count)
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
            _indexGroup = -1;
            _array = _db.QueryEntities<T>(_groups[0], out _count);
        }
        
        public ref T Current => ref _array[_index];
        
        T IEnumerator<T>.  Current   =>  throw new System.NotImplementedException();
        object IEnumerator.Current   =>  throw new System.NotImplementedException();
        public void        Dispose() {}

        readonly IEntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;
        T[]                       _array;
        uint                      _count;
        int                      _index;
        int                      _indexGroup;
        
    }
    
    public struct EntityIterator<T>:IEnumerator<T>
    {
        public EntityIterator(T[] array, uint count):this()
        {
            _array = array;
            _count = count;
            _index = -1;
        }

        public bool MoveNext()
        {
            return ++_index < _count;
        }
        public void Reset()    { _index = -1; }
        
        public ref T Current => ref _array[_index];
        
        T IEnumerator<T>.  Current   =>  throw new System.NotImplementedException();
        object IEnumerator.Current   =>  throw new System.NotImplementedException();
        public void        Dispose() { }
        
        readonly T[]  _array;
        readonly uint _count;
        int          _index;
    }
}