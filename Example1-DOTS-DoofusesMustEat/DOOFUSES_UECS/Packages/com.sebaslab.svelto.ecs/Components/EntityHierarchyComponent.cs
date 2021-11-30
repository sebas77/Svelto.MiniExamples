namespace Svelto.ECS
{
    public struct EntityHierarchyComponent: IEntityComponent, INeedEGID
    {
        public readonly ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyComponent(ExclusiveGroupStruct group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}