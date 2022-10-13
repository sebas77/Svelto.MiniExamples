using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public class CameraEntityDescriptor : GenericEntityDescriptor<PositionComponent, CameraTargetEntityReferenceComponent>
    {
    }

    public struct PositionComponent : IEntityComponent
    {
        public Vector3 position;
    }
}