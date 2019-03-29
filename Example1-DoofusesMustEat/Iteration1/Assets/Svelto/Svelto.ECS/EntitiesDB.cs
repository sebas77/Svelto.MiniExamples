#if DEBUG && !PROFILER
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB : IEntitiesDB
    {
        internal EntitiesDB(FasterDictionary<uint, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsDB,
            Dictionary<Type, FasterDictionary<uint, ITypeSafeDictionary>> groupedGroups, EntitiesStream entityStream)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupedGroups = groupedGroups;
            _entityStream = entityStream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(uint group) where T:class, IEntityStruct
        {
            if (QueryEntitySafeDictionary(group, out TypeSafeDictionary<T> typeSafeDictionary) == false)
                return new ReadOnlyCollectionStruct<T>(RetrieveEmptyEntityViewArray<T>(), 0);

            return typeSafeDictionary.Values;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyCollectionStruct<T> QueryEntityViews<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct
        {
            return QueryEntityViews<T>((uint) group);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T QueryEntityView<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct
        {
            return QueryEntityView<T>(new EGID(id, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryUniqueEntity<T>(uint @group) where T : IEntityStruct
        {
            var entities = QueryEntities<T>(group, out var count);

            if (count != 1) throw new ECSException("Unique entities must be unique! ".FastConcat(typeof(T).ToString()));

            return ref entities[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct @group) where T : IEntityStruct
        {
            return ref QueryUniqueEntity<T>((uint) @group);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(EGID entityGID) where T : IEntityStruct
        {
            T[]  array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out var index)) != null)
                return ref array[index];

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(uint id, uint group) where T : IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntities<T>(uint group, out uint count) where T : IEntityStruct
        {
            count = 0;
            if (QueryEntitySafeDictionary(group, out TypeSafeDictionary<T> typeSafeDictionary) == false)
                return RetrieveEmptyEntityViewArray<T>();

            return typeSafeDictionary.GetValuesArray(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count) where T : IEntityStruct
        {
            return QueryEntities<T>((uint) groupStruct, out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[]) QueryEntities<T1, T2>(uint @group, out uint count) where T1 : IEntityStruct where T2 : IEntityStruct
        {
            var T1entities = QueryEntities<T1>(group, out var countCheck);
            var T2entities = QueryEntities<T2>(group, out count);

            if (count != countCheck)
                throw new ECSException("Entity views count do not match in group. Entity 1: ".
                                       FastConcat(typeof(T1).ToString()).FastConcat(
                                       "Entity 2: ".FastConcat(typeof(T2).ToString())));


            return (T1entities, T2entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[]) QueryEntities<T1, T2>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct
        {
            return QueryEntities<T1, T2>((uint) groupStruct, out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(uint @group, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct where T3 : IEntityStruct
        {
            var T1entities = QueryEntities<T1>(group, out var countCheck1);
            var T2entities = QueryEntities<T2>(group, out var countCheck2);
            var T3entities = QueryEntities<T3>(group, out count);

            if (count != countCheck1 || count != countCheck2)
                throw new ECSException("Entity views count do not match in group. Entity 1: ".
                                       FastConcat(typeof(T1).ToString()).
                                       FastConcat(" Entity 2: ".FastConcat(typeof(T2).ToString()).
                                       FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))));

            return (T1entities, T2entities, T3entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (T1[], T2[], T3[]) QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct, out uint count)
            where T1 : IEntityStruct where T2 : IEntityStruct where T3 : IEntityStruct
        {
            return QueryEntities<T1, T2, T3>((uint) groupStruct, out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(uint groupID) where T : IEntityStruct
        {
            TypeSafeDictionary<T> typeSafeDictionary;

            if (QueryEntitySafeDictionary(groupID, out typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(groupID, typeof(T));

            EGIDMapper<T> mapper;
            mapper.map = typeSafeDictionary;

            uint count;
            typeSafeDictionary.GetValuesArray(out count);

            return mapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId) where T : IEntityStruct
        {
            return QueryMappedEntities<T>((uint) groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array) where T : IEntityStruct
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index) where T : IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, group), out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array) where T : IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, group), out index, out array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(uint id, uint group, out uint index) where T : IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, group), out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(uint id, uint group, out uint index, out T[] array) where T : IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, group), out index, out array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T QueryEntityView<T>(EGID entityGID) where T : class, IEntityStruct
        {
            if (TryQueryEntityViewInGroupInternal(entityGID, out T entityView) == false)
                throw new EntityNotFoundException(entityGID, typeof(T));

            return entityView;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : IEntityStruct
        {
            if (QueryEntitySafeDictionary(entityGID.groupID, out TypeSafeDictionary<T> casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, uint groupid) where T : IEntityStruct
        {
            return Exists<T>(new EGID(id, groupid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid)
        {
            return _groupEntityViewsDB.ContainsKey(gid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(uint group) where T : IEntityStruct
        {
            QueryEntities<T>(group, out var count);
            return count > 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : IEntityStruct
        {
            return HasAny<T>((uint) groupStruct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : IEntityStruct
        {
            QueryEntities<T>(groupStruct, out var count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>(uint groupStruct) where T : IEntityStruct
        {
            QueryEntities<T>(groupStruct, out var count);
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityStruct
        {
            _entityStream.PublishEntity(ref QueryEntity<T>(egid));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntityView<T>(EGID entityegid, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroupInternal(entityegid, out entityView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntityView<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out T entityView) where T : class, IEntityStruct
        {
            return TryQueryEntityViewInGroupInternal(new EGID(id, (uint) group), out entityView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryQueryEntityViewInGroupInternal<T>(EGID entityGID, out T entityView) where T:class, IEntityStruct
        {
            entityView = null;
            if (QueryEntitySafeDictionary(entityGID.groupID, out TypeSafeDictionary<T> safeDictionary) == false) return false;

            return safeDictionary.TryGetValue(entityGID.entityID, out entityView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : IEntityStruct
        {
            index = 0;
            if (QueryEntitySafeDictionary(entityGID.groupID, out TypeSafeDictionary<T> safeDictionary) == false)
                return null;

            if (safeDictionary.TryFindElementIndex(entityGID.entityID, out index) == false)
                return null;

            return safeDictionary.GetValuesArray(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool QueryEntitySafeDictionary<T>(uint group, out TypeSafeDictionary<T> typeSafeDictionary) where T : IEntityStruct
        {
            typeSafeDictionary = null;

            //search for the group
            if (_groupEntityViewsDB.TryGetValue(group, out var entitiesInGroupPerType) == false)
                return false;

            //search for the indexed entities in the group
            if (entitiesInGroupPerType.TryGetValue(typeof(T), out var safeDictionary) == false)
                return false;

            //return the indexes entities if they exist
            typeSafeDictionary = (safeDictionary as TypeSafeDictionary<T>);

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlyCollectionStruct<T> RetrieveEmptyEntityViewList<T>()
        {
            var arrayFast = FasterList<T>.DefaultList.ToArrayFast();

            return new ReadOnlyCollectionStruct<T>(arrayFast, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return FasterList<T>.DefaultList.ToArrayFast();
        }

        //grouped set of entity views, this is the standard way to handle entity views entity views are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity views of
        //a specific type inside a specific group.
        readonly FasterDictionary<uint, Dictionary<Type, ITypeSafeDictionary>> _groupEntityViewsDB;
        //needed to be able to iterate over all the entities of the same type regardless the group
        //may change in future
        readonly Dictionary<Type, FasterDictionary<uint, ITypeSafeDictionary>> _groupedGroups;
        readonly EntitiesStream _entityStream;
    }
}
