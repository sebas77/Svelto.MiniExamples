using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    /// <summary>
    /// This design is old and I have never refactored it (also because the demo is pretty simple)
    /// I guess it would make more sense to not create fat components with multiple responsibility like this
    /// but instead to have separate components that are used when needed. This component looks more like an
    /// entity itself.
    /// </summary>
    public struct EnemyEntityViewComponent : IEntityViewComponent
    {
        public IEnemyMovementComponent movementComponent;
        public IEnemyVFXComponent      vfxComponent;
        public IEnemyTriggerComponent  targetTriggerComponent;

        public IAnimationComponent    animationComponent;
        public ITransformComponent    transformComponent;
        public IPositionComponent     positionComponent;
        public ILayerComponent        layerComponent;

        public EGID ID { get; set; }
    }
}