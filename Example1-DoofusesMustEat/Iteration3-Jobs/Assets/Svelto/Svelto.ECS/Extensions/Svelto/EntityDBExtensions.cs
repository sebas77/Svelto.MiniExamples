namespace Svelto.ECS
{
    public static class EntityDBExtensionsNative
    {
       public static AllGroupsEnumerable<T1> QueryEntities<T1>(this EntitiesDB db)
            where T1 :struct, IEntityComponent
        {
            return new AllGroupsEnumerable<T1>(db);
        }
    }
}