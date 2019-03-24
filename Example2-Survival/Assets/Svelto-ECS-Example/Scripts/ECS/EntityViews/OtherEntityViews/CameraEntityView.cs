namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraEntityView: IEntityViewStruct
    {
        public ITransformComponent transformComponent;
        public IPositionComponent  positionComponent;
        
        public EGID ID { get; set; }
    }
}