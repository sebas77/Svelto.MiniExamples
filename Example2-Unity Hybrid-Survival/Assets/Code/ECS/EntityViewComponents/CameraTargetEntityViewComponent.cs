using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraTargetEntityViewComponent : IEntityViewComponent
    {
        public readonly IPositionComponent targetComponent;
        
        public EGID                   ID { get; set; }
    }
}