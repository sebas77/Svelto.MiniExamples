using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public struct EnemyTargetEntityViewComponent : IEntityViewComponent
    {
        public IPositionComponent targetPositionComponent;
        
        public EGID ID { get; set; }
    }
}