using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class EntityDBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static AllGroupsEnumerable<T1> QueryEntities<T1>(this EntitiesDB db)
            where T1 :struct, IEntityComponent
        {
            return new AllGroupsEnumerable<T1>(db);
        }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static NB<T> QueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index) where T : unmanaged, IEntityComponent
       {
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out NB<T> array) == true)
               return array;

           throw new EntityNotFoundException(entityGID, typeof(T));
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> array)
           where T : unmanaged, IEntityComponent
       {
           if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out array) == true)
               return true;

           return false;
       }
        
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       static bool QueryEntitiesAndIndexInternal<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out NB<T> buffer) where T : unmanaged, IEntityComponent
       {
           index = 0;
           buffer = default;
           if (entitiesDb.SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
               return false;

           if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
               return false;
            
           buffer = (NB<T>) (safeDictionary as ITypeSafeDictionary<T>).GetValues(out _);

           return true;
       }
       
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, EGID entityGID) where T : unmanaged, IEntityComponent
       {
           var array = QueryEntitiesAndIndex<T>(entitiesDb, entityGID, out var index);
           
           return ref array[(int) index];
       }

       [MethodImpl(MethodImplOptions.AggressiveInlining)]
       public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group) where T : unmanaged, IEntityComponent
       {
           return ref QueryEntity<T>(entitiesDb, new EGID(id, group));
       }
    }

    public static class EntityDBExtensionsB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<T> QueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index) where T : struct, IEntityViewComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out MB<T> array) == true)
                return array;

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryEntitiesAndIndex<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out MB<T> array)
            where T : struct, IEntityViewComponent
        {
            if (entitiesDb.QueryEntitiesAndIndexInternal<T>(entityGID, out index, out array) == true)
                return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool QueryEntitiesAndIndexInternal<T>(this EntitiesDB entitiesDb, EGID entityGID, out uint index, out MB<T> buffer) where T : struct, IEntityViewComponent
        {
            index = 0;
            if (entitiesDb.SafeQueryEntityDictionary<T>(entityGID.groupID, out var safeDictionary) == false)
                return false;

            if (safeDictionary.TryFindIndex(entityGID.entityID, out index) == false)
                return false;
            
            buffer = (MB<T>) (safeDictionary as ITypeSafeDictionary<T>).GetValues(out _);

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, EGID entityGID) where T : struct, IEntityViewComponent
        {
            var array = QueryEntitiesAndIndex<T>(entitiesDb, entityGID, out var index);
           
            return ref array[(int) index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T QueryEntity<T>(this EntitiesDB entitiesDb, uint id, ExclusiveGroupStruct group) where T : struct, IEntityViewComponent
        {
            return ref QueryEntity<T>(entitiesDb, new EGID(id, group));
        }
    }
}