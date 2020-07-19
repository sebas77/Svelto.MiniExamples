using Svelto.ECS.Components;

namespace Svelto.ECS.EntityComponents
{
    public struct PositionEntityComponent : IEntityComponent
    {
        public ECSVector3 position;
    }
}