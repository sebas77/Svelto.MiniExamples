using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyEntityViewStruct : IEntityViewStruct
    {
        public IEnemyMovementComponent movementComponent;
        public IEnemyVFXComponent      vfxComponent;

        public IAnimationComponent animationComponent;
        public ITransformComponent transformComponent;
        public IPositionComponent  positionComponent;
        public ILayerComponent     layerComponent;

        public EGID ID { get; set; }
    }

    public struct EnemyAttackEntityViewStruct : IEntityViewStruct
    {
        public IEnemyTriggerComponent targetTriggerComponent;
        public EGID                   ID { get; set; }
    }

    public struct EnemyTargetEntityViewStruct : IEntityViewStruct
    {
        public IPositionComponent targetPositionComponent;
        public EGID               ID { get; set; }
    }
}