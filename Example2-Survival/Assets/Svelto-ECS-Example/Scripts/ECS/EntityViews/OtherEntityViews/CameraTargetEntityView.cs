namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraTargetEntityView: IEntityViewStruct
    {
        public ICameraTargetComponent targetComponent;
        public EGID ID { get; set; }
    }
}