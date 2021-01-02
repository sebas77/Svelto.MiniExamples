namespace Svelto.ECS.Example.Survive.Characters
{
    /// <summary>
    /// What happens when the health reach 0? The fact the entity "dies" is one consequence. I could have merged
    /// this engine with ApplyDamageEngine, but I decided to split it to show that the consequence of something
    /// happening may not have to happen in the same code. This engine is also taking the responsibility of deciding
    /// if the entity must die or not and will communicate it through the use of the Publisher/Consumer pattern.
    /// </summary>
    public class DispatchKilledEntitiesEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { set; private get; }

        public void Step()
        {
            foreach (var ((buffer, count), _) in entitiesDB.QueryEntities<HealthComponent>(
                ECSGroups.DamageableEntitiesGroups))
            {
                for (int i = 0; i < count; ++i)
                    if (buffer[i].currentHealth <= 0)
                    {
                        entitiesDB.PublishEntityChange<DeathComponent>(buffer[i].ID);
                        
                        Svelto.Console.LogDebug(("death " + buffer[i].ID));
                    }
            }
        }

        public string name => nameof(DispatchKilledEntitiesEngine);
    }
}