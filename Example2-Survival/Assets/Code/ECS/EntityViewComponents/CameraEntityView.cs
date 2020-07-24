using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraEntityView : IEntityViewComponent
    {
        public ITransformComponent transformComponent;
        public IPositionComponent  positionComponent;

        public EGID ID { get; set; }
    }
}