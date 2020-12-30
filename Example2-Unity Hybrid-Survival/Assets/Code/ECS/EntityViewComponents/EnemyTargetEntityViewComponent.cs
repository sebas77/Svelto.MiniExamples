using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyTargetEntityViewComponent : IEntityViewComponent
    {
        public IPositionComponent targetPositionComponent;
        
        public EGID ID { get; set; }
    }
}