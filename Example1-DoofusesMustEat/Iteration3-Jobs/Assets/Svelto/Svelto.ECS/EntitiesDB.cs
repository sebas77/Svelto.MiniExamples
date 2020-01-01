#if DEBUG && !PROFILER
#define ENABLE_DEBUG_FUNC
#endif

using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class EntitiesDB : IEntitiesDB
    {
        internal EntitiesDB(
            FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> groupEntityViewsDB,
            FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> groupsPerEntity,
            EntitiesStream entityStream)
        {
            _groupEntityViewsDB = groupEntityViewsDB;
            _groupsPerEntity = groupsPerEntity;
            _entityStream = entityStream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryUniqueEntity<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            var entities = QueryEntities<T>(group).ToFastAccess(out var count);

#if DEBUG && !PROFILER
            if (count == 0)
                throw new ECSException("Unique entity not found '".FastConcat(typeof(T).ToString()).FastConcat("'"));
            if (entities.Length != 1)
                throw new ECSException("Unique entities must be unique! '".FastConcat(typeof(T).ToString())
                    .FastConcat("'"));
#endif
            return ref entities[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T QueryEntity<T>(EGID entityGID) where T : struct, IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out var index)) != null)
                return ref array[(int) index];

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        public ref T QueryEntity<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            return ref QueryEntity<T>(new EGID(id, group));
        }

        public EntityCollection<T, ManagedBuffer<T>> QueryEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityStruct
        {
            T[] ret;
            uint      count = 0;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                ret = RetrieveEmptyEntityViewArray<T>();
            else
                ret = (typeSafeDictionary as ITypeSafeDictionary<T>).GetValuesArray(out count);

            return new EntityCollection<T, ManagedBuffer<T>>(ret, count);
        }

        public EntityCollection<T1, T2, ManagedBuffer<T1>, ManagedBuffer<T2>> QueryEntities<T1, T2>(
            ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
        {
            var T1entities = QueryEntities<T1>(groupStruct);
            var T2entities = QueryEntities<T2>(groupStruct);

            if (T1entities.length != T2entities.length)
                throw new ECSException("Entity views count do not match in group. Entity 1: ' count: "
                    .FastConcat(T1entities.length).FastConcat(typeof(T1).ToString())
                    .FastConcat("'. Entity 2: ' count: ".FastConcat(T2entities.length)
                        .FastConcat(typeof(T2).ToString())
                        .FastConcat("'")));

            return new EntityCollection<T1, T2, ManagedBuffer<T1>, ManagedBuffer<T2>>(T1entities, T2entities);
        }

        public EntityCollection<T1, T2, T3, ManagedBuffer<T1>, ManagedBuffer<T2>, ManagedBuffer<T3>>
            QueryEntities<T1, T2, T3>(ExclusiveGroup.ExclusiveGroupStruct groupStruct)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct where T3 : struct, IEntityStruct
        {
            var T1entities = QueryEntities<T1>(groupStruct);
            var T2entities = QueryEntities<T2>(groupStruct);
            var T3entities = QueryEntities<T3>(groupStruct);

            if (T1entities.length != T2entities.length || T2entities.length != T3entities.length)
                throw new ECSException("Entity views count do not match in group. Entity 1: "
                    .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                    .FastConcat(T1entities.length)
                    .FastConcat(" Entity 2: "
                        .FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                        .FastConcat(T2entities.length)
                        .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                        .FastConcat(" count: ").FastConcat(T3entities.length)));

            return new EntityCollection<T1, T2, T3, ManagedBuffer<T1>, ManagedBuffer<T2>, ManagedBuffer<T3>>(T1entities,
                T2entities, T3entities);
        }

        public EntityCollections<T> QueryEntities<T>(ExclusiveGroup[] groups) where T : struct, IEntityStruct
        {
            return new EntityCollections<T>(this, groups);
        }

        public EntityCollections<T1, T2> QueryEntities<T1, T2>(ExclusiveGroup[] groups)
            where T1 : struct, IEntityStruct where T2 : struct, IEntityStruct
        {
            return new EntityCollections<T1, T2>(this, groups);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(groupStructId, typeof(T));

            return (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEGIDMapper<T> QueryNativeMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityStruct
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(groupStructId, typeof(T));

            return (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEGIDMapper<T> QueryNativeMappedEntities<T>(FastGroup.ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityStruct
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(groupStructId, typeof(T));

            return (typeSafeDictionary as FastTypeSafeDictionary<T>).ToNativeEGIDMapper<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
            out EGIDMapper<T> mapper)
            where T : struct, IEntityStruct
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                return false;

            mapper = (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper();

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryNativeMappedEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
            out NativeEGIDMapper<T> mapper)
            where T : unmanaged, IEntityStruct
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                return false;

            mapper = (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper();

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryNativeMappedEntities<T>(FastGroup.ExclusiveGroupStruct groupStructId,
                                                    out NativeEGIDMapper<T>             mapper)
            where T : unmanaged, IEntityStruct
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                return false;

            mapper = (typeSafeDictionary as FastTypeSafeDictionary<T>).ToNativeEGIDMapper();

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(EGID entityGID, out uint index) where T : struct, IEntityStruct
        {
            T[] array;
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGID, out index)) != null)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex<T>(EGID entityGid, out uint index, out T[] array)
            where T : struct, IEntityStruct
        {
            if ((array = QueryEntitiesAndIndexInternal<T>(entityGid, out index)) != null)
                return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] QueryEntitiesAndIndex<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct @group, out uint index)
            where T : struct, IEntityStruct
        {
            return QueryEntitiesAndIndex<T>(new EGID(id, @group), out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryEntitiesAndIndex
            <T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index, out T[] array)
            where T : struct, IEntityStruct
        {
            return TryQueryEntitiesAndIndex(new EGID(id, @group), out index, out array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var casted) == false) return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, ExclusiveGroup.ExclusiveGroupStruct group) where T : struct, IEntityStruct
        {
            if (SafeQueryEntityDictionary<T>(group, out var casted) == false) return false;

            return casted != null && casted.ContainsKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(ExclusiveGroup.ExclusiveGroupStruct gid)
        {
            return _groupEntityViewsDB.ContainsKey(gid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct
        {
            return QueryEntities<T>(groupStruct).length > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>(ExclusiveGroup.ExclusiveGroupStruct groupStruct) where T : struct, IEntityStruct
        {
            return QueryEntities<T>(groupStruct).length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishEntityChange<T>(EGID egid) where T : unmanaged, IEntityStruct
        {
            _entityStream.PublishEntity(ref QueryEntity<T>(egid), egid);
        }

        public void ExecuteOnAllEntities<T>(ExecuteOnAllEntitiesAction<T> action) where T : struct, IEntityStruct
        {
            var type = typeof(T);

            if (_groupsPerEntity.TryGetValue(new RefWrapper<Type>(type), out var dictionary))
                foreach (var pair in dictionary)
                {
                    var entities = (pair.Value as ITypeSafeDictionary<T>).GetValuesArray(out var count);

                    if (count > 0)
                        action(entities, new ExclusiveGroup.ExclusiveGroupStruct(pair.Key), count, this);
                }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, ExecuteOnAllEntitiesAction<T, W> action)
            where T : struct, IEntityStruct
        {
            var type = typeof(T);

            if (_groupsPerEntity.TryGetValue(new RefWrapper<Type>(type), out var dic))
                foreach (var pair in dic)
                {
                    var entities = (pair.Value as ITypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, new ExclusiveGroup.ExclusiveGroupStruct(pair.Key), innerCount, this,
                               ref value);
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T[] QueryEntitiesAndIndexInternal<T>(EGID entityGID, out uint index) where T : struct, IEntityStruct
        {
            index = 0;
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
                return null;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return null;

            return (safeDictionary as ITypeSafeDictionary<T>).unsafeValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool SafeQueryEntityDictionary<T>(uint group, out ITypeSafeDictionary typeSafeDictionary)
            where T : struct, IEntityStruct
        {
            if (UnsafeQueryEntityDictionary(group, TypeCache<T>.type, out var safeDictionary) == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool UnsafeQueryEntityDictionary(uint group, Type type, out ITypeSafeDictionary typeSafeDictionary)
        {
            //search for the group
            if (_groupEntityViewsDB.TryGetValue(group, out var entitiesInGroupPerType) == false)
            {
                typeSafeDictionary = null;
                return false;
            }

            //search for the indexed entities in the group
            return entitiesInGroupPerType.TryGetValue(new RefWrapper<Type>(type), out typeSafeDictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T[] RetrieveEmptyEntityViewArray<T>()
        {
            return EmptyList<T>.emptyArray;
        }

        static class EmptyList<T>
        {
            internal static readonly T[] emptyArray = new T[0];
        }
        
        readonly EntitiesStream _entityStream;

        //grouped set of entity views, this is the standard way to handle entity views entity views are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity views of
        //a specific type inside a specific group.
        readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> _groupEntityViewsDB;

        //needed to be able to iterate over all the entities of the same type regardless the group
        //may change in future
        readonly FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> _groupsPerEntity;
    }
}