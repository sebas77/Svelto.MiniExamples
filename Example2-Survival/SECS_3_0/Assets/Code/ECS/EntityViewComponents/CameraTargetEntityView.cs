using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraTargetEntityView : IEntityViewComponent
    {
        public IPositionComponent targetComponent;
        public EGID                   ID { get; set; }
    }
}