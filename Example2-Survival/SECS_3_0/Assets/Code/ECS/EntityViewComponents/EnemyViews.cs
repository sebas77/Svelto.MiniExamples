using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyEntityViewComponent : IEntityViewComponent
    {
        public IEnemyMovementComponent movementComponent;
        public IEnemyVFXComponent      vfxComponent;

        public IAnimationComponent animationComponent;
        public ITransformComponent transformComponent;
        public IPositionComponent  positionComponent;
        public ILayerComponent     layerComponent;

        public EGID ID { get; set; }
    }

    public struct EnemyAttackEntityViewComponent : IEntityViewComponent
    {
        public IEnemyTriggerComponent targetTriggerComponent;
        public EGID                   ID { get; set; }
    }

    public struct EnemyTargetEntityViewComponent : IEntityViewComponent
    {
        public IPositionComponent targetPositionComponent;
        public EGID               ID { get; set; }
    }
}