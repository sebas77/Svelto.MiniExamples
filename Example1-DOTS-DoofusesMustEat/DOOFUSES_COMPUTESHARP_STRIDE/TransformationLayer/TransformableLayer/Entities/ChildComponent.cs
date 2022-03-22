namespace Svelto.ECS.MiniExamples.Turrets
{
    public struct ChildComponent : IEntityComponent
    {
        public ChildComponent(EntityReference parent) { this.parent = parent; }

        public EntityReference parent;
    }
}