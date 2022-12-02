using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Transformable;

namespace Svelto.ECS.Example.Survive.Camera
{
    public class CameraEntityDescriptor: GenericEntityDescriptor<PositionComponent, CameraTargetEntityReferenceComponent
      , CameraEntityComponent, GameObjectEntityComponent> { }
}