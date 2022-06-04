using Svelto.ECS;

namespace Svelto.ECS
{
    public struct EntityHierarchyComponent : IEntityComponent
    {
        public ExclusiveGroupStruct parentGroup;
        
        public EntityHierarchyComponent(ExclusiveGroupStruct parentGroup)
        {
            this.parentGroup = parentGroup;
        }
    }
}