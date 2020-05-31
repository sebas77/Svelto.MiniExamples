using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class EntityDBExtensionsNative
    {
        // public static NativeGroupsEnumerable<T1, T2> QueryGroupedNativeEntities<T1, T2>(this EntitiesDB db,
        //                                                                                 FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        // {
        //     return new NativeGroupsEnumerable<T1, T2>(db, groups);
        // }
        //
        // public static NativeGroupsEnumerable<T1, T2, T3> QueryGroupedNativeEntities
        //     <T1, T2, T3>(this EntitiesDB db, FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        //     where T3 : unmanaged, IEntityComponent
        // {
        //     return new NativeGroupsEnumerable<T1, T2, T3>(db, groups);
        // }
        //
        // public static NativeGroupsEnumerable<T1, T2, T3, T4> QueryGroupedNativeEntities
        //     <T1, T2, T3, T4>(this EntitiesDB db, FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        //     where T3 : unmanaged, IEntityComponent where T4 : unmanaged, IEntityComponent
        // {
        //     return new NativeGroupsEnumerable<T1, T2, T3, T4>(db, groups);
        // }
        //
        // public static NativeGroupsEnumerable<T1> QueryGroupedNativeEntities<T1>(this EntitiesDB db, FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : unmanaged, IEntityComponent
        // {
        //     return new NativeGroupsEnumerable<T1>(db, groups);
        // }
        //
        // public static NativeAllGroupsEnumerable<T1> QueryGroupedNativeEntities<T1>(this EntitiesDB db)
        //     where T1 : unmanaged, IEntityComponent
        // {
        //     return new NativeAllGroupsEnumerable<T1>(db);
        // }
        
        // public static BT<NB<T>> QueryNativeEntitiesBuffer<T>(this EntitiesDB db, ExclusiveGroupStruct @group)
        //     where T : unmanaged, IEntityComponent
        // {
        //     return db.QueryEntities<T>(group).ToBuffer();
        // }
        //
        // public static BT<NB<T1>, NB<T2>> QueryNativeEntitiesBuffer<T1, T2>(this EntitiesDB db, ExclusiveGroupStruct @group)
        //     where T1 : unmanaged, IEntityComponent
        //     where T2 : unmanaged, IEntityComponent
        // {
        //     return db.QueryEntities<T1, T2>(group).ToBuffer();
        // }
        //
        // public static BT<NB<T1>, NB<T2>, NB<T3>> QueryNativeEntitiesBuffer<T1, T2, T3>(this EntitiesDB db, ExclusiveGroupStruct @group)
        //     where T1 : unmanaged, IEntityComponent
        //     where T2 : unmanaged, IEntityComponent
        //     where T3 : unmanaged, IEntityComponent
        // {
        //     return db.QueryEntities<T1, T2, T3>(group).ToBuffer();
        // }
    }

    public static class EntityDBExtensions
    {
        // public static GroupsEnumerable<T1, T2> QueryGroupedEntities<T1, T2>(this EntitiesDB db,
        //                                                                     FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        // {
        //     return new GroupsEnumerable<T1, T2>(db, groups);
        // }
        //
        // public static GroupsEnumerable<T1, T2, T3> QueryGroupedEntities
        //     <T1, T2, T3>(this EntitiesDB db, FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        //     where T3 : struct, IEntityComponent
        // {
        //     return new GroupsEnumerable<T1, T2, T3>(db, groups);
        // }
        //
        // public static GroupsEnumerable<T1, T2, T3, T4> QueryGroupedEntities
        //     <T1, T2, T3, T4>(this EntitiesDB db, FasterList<ExclusiveGroupStruct> groups)
        //     where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
        //     where T3 : struct, IEntityComponent where T4 : struct, IEntityComponent
        // {
        //     return new GroupsEnumerable<T1, T2, T3, T4>(db, groups);
        // }
        
        public static AllGroupsEnumerable<T1> QueryEntities<T1>(this EntitiesDB db)
            where T1 :struct, IEntityComponent
        {
            return new AllGroupsEnumerable<T1>(db);
        }
    }
}