namespace Svelto.ECS.Example.Survive.Damage
{
    /// <summary>
    /// What happens when the health reach 0? The fact the entity "dies" is one consequence. I could have merged
    /// this engine with ApplyDamageEngine, but I decided to split it to show that the consequence of something
    /// happening may not have to happen in the same code. This engine is also taking the responsibility of deciding
    /// if the entity must die or not and will communicate it through the use of the Publisher/Consumer pattern.
    /// </summary>
    public class KilledEntitiesEngine : IQueryingEntitiesEngine, IStepEngine
    {
        EntitiesDB.SveltoFilters _sveltoFilters;

        public void Ready()
        {
            _sveltoFilters = entitiesDB.GetFilters();
            _sveltoFilters.CreateTransientFilter<HealthComponent>(FilterIDs.deadEntitiesFilter);
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Step()
        {
            var damagedEntitiesFilter = _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.damagedEntitiesFilter);
            var deadEntitiesFilter = _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.deadEntitiesFilter);
            
            foreach (var (filteredIndices, group) in damagedEntitiesFilter)
            {
                var (health, entityIDs, _) = entitiesDB.QueryEntities<HealthComponent>(group);

                var indicesCount = filteredIndices.count;
                for (int i = 0; i < indicesCount; ++i)
                    //filters subset groups using double indexing. It's VERY important to use the double indexing and not i directly
                {
                    var filteredIndex = filteredIndices[i];
                    if (health[filteredIndex].currentHealth <= 0)
                    {
                        deadEntitiesFilter.Add(new EGID(entityIDs[filteredIndex], group), (uint)filteredIndex);
                    }
                }
            }
        }

        public string name => nameof(KilledEntitiesEngine);
    }
}