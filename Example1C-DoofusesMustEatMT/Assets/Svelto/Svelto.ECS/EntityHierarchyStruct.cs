namespace Svelto.ECS
{
    public struct EntityHierarchyStruct : IEntityStruct
    {
        public readonly ExclusiveGroup.ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyStruct(ExclusiveGroup @group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}