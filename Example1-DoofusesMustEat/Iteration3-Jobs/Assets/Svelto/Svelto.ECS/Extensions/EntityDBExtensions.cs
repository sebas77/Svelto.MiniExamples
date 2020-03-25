using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class EntityDBExtensions
    {
        public static NativeGroupsEnumerable<T1, T2> NativeGroupsIterator<T1, T2>(this EntitiesDB db,
            ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1, T2>(db, groups);
        }

        public static NativeGroupsEnumerable<T1, T2, T3> NativeGroupsIterator
            <T1, T2, T3>(this EntitiesDB db, ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1, T2, T3>(db, groups);
        }

        public static NativeAllGroupsEnumerable<T1> NativeGroupsIterator<T1>(this EntitiesDB db)
            where T1 : unmanaged, IEntityComponent
        {
            return new NativeAllGroupsEnumerable<T1>(db);
        }

        public static NativeBuffer<T> NativeEntitiesBuffer<T>(this EntitiesDB db, ExclusiveGroup @group, out uint count)
            where T : unmanaged, IEntityComponent
        {
            return db.QueryEntities<T>(group).ToNativeBuffer<T>(out count);
        }
    }
}