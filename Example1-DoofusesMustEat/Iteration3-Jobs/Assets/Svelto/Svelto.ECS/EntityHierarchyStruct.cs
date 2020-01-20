namespace Svelto.ECS
{
    public struct EntityHierarchyStruct: IEntityStruct, INeedEGID
    {
        public readonly ExclusiveGroupStruct parentGroup;
            
        public EntityHierarchyStruct(ExclusiveGroup group): this() { parentGroup = group; }
            
        public EGID ID { get; set; }
    }
}