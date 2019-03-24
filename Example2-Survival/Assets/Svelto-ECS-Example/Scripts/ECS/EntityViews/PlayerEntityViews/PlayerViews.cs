namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public struct PlayerEntityViewStruct : IEntityViewStruct
    {
        public ISpeedComponent         speedComponent;
        public IRigidBodyComponent     rigidBodyComponent;
        public IPositionComponent      positionComponent;
        public IAnimationComponent     animationComponent;
        public ITransformComponent     transformComponent;
        public EGID ID { get; set; }
    }
}

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public struct GunEntityViewStruct : IEntityViewStruct
    {
        public IGunAttributesComponent   gunComponent;
        public IGunFXComponent           gunFXComponent;
        public IGunHitTargetComponent    gunHitTargetComponent;
        public EGID ID { get; set; }
    }
}
