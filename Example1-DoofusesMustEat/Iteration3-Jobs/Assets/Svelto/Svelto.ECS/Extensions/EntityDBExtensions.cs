namespace Svelto.ECS
{
    public static class EntityDBExtensions
    {
        public static NativeGroupsEnumerable<T1, T2> GroupsIterator<T1, T2>(this EntitiesDB db, ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct
        {
            return new NativeGroupsEnumerable<T1, T2>(db, groups);
        }

        public static NativeGroupsEnumerable<T1, T2, T3> GroupsIterator
            <T1, T2, T3>(this EntitiesDB db, ExclusiveGroup[] groups)
            where T1 : unmanaged, IEntityStruct where T2 : unmanaged, IEntityStruct where T3 : unmanaged, IEntityStruct
        {
            return new NativeGroupsEnumerable<T1, T2, T3>(db, groups);
        }
        
         public static NativeAllGroupsEnumerable<T1> GroupsIterator<T1>(this EntitiesDB db) where T1 : unmanaged, IEntityStruct
         {
             return new NativeAllGroupsEnumerable<T1>(db);
         }
    }
}
