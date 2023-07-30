using Svelto.ECS;

namespace Logic.SveltoECS
{
    public struct TargetDC: IEntityComponent
    {
        public EntityReference target;
    }
}