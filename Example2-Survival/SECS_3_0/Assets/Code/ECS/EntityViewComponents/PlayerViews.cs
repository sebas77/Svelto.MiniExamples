using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public struct PlayerEntityViewComponent : IEntityViewComponent
    {
        public ISpeedComponent     speedComponent;
        public IRigidBodyComponent rigidBodyComponent;
        public IPositionComponent  positionComponent;
        public IAnimationComponent animationComponent;
        public ITransformComponent transformComponent;
        public EGID                ID { get; set; }
    }
}

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public struct GunEntityViewComponent : IEntityViewComponent
    {
        public IGunFXComponent         gunFXComponent;
        public EGID                    ID { get; set; }
    }
}