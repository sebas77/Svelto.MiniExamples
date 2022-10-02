using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraEntityViewComponent : IEntityViewComponent
    {
        public ITransformComponent transformComponent;
        public IPositionComponent  positionComponent;
        public ICameraComponent    cameraComponent;

        public EGID                ID { get; set; }
    }
}