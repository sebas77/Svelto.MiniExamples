using Svelto.Common;
using Svelto.ECS.Extensions;

namespace Svelto.ECS.Example.Survive.Characters
{
    /// <summary>
    /// What happens when the health reach 0? The fact the entity "dies" is one consequence. I could have merged
    /// this engine with ApplyDamageEngine, but I decided to split it to show that the consequence of something
    /// happening may not have to happen in the same code.
    /// </summary>
    [Sequenced(nameof(EnginesEnum.DeathEngine))]
    public class DeathEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { set; private get; }

        public void Step()
        {
            var healths = entitiesDB.QueryEntities<HealthComponent>(ECSGroups.DamageableEntities).entities;

            foreach (ref var health in healths)
            {
                if (health.currentHealth <= 0)
                    entitiesDB.PublishEntityChange<DeathComponent>(health.ID);
            }
        }

        public string name => nameof(DeathEngine);
    }
}