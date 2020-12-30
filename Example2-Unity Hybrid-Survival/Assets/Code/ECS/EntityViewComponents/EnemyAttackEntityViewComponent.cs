using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyAttackEntityViewComponent : IEntityViewComponent
    {
        public IEnemyTriggerComponent targetTriggerComponent;
        
        public EGID ID { get; set; }
    }
}