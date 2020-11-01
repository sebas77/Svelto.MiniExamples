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
            foreach (var ((buffer, count), _) in entitiesDB.QueryEntities<HealthComponent>(ECSGroups.DamageableEntities))
            {
                for (int i = 0; i < count; ++i)
                    if (buffer[i].currentHealth <= 0)
                        entitiesDB.PublishEntityChange<DeathComponent>(buffer[i].ID);
            }
        }

        public string name => nameof(DeathEngine);
    }
}