using Svelto.ECS.Components;

namespace Svelto.ECS.EntityComponents
{
    public struct RotationEntityComponent : IEntityComponent
    {
        public ECSQuaternion rotation;
    }
}