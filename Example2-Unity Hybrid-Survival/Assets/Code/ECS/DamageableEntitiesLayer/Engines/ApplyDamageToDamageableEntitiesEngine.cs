namespace Svelto.ECS.Example.Survive.Damage
{
    /// <summary>
    ///     The responsibility of this engine is to apply the damage to any damageable entity. The behaviour can
    ///     be in common with any entity as multiple parameters could be added to differentiate outcome between
    ///     entities through data. 
    ///     In my articles I introduce the concept of layered design, where several layers of abstractions can
    ///     co-exist. Every abstracted layer can be seen as a "framework" for the more specialized layers.
    ///     This would be part of an hypothetical "damageable entities" framework that could be distributed
    ///     independently by the specialised entities and reused in multiple projects.
    /// </summary>
    public class ApplyDamageToDamageableEntitiesEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public void Ready()
        {
            _sveltoFilters = entitiesDB.GetFilters();
            _sveltoFilters.CreateTransientFilter<HealthComponent>(FilterIDs.damagedEntitiesFilter);
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Step()
        {
            //Events are not a good way to create subset of entities found in a given state. With Svelto.ECS
            //there are several approaches to organise subset of entities:
            //statically, using components
            //dynamically using group tags in group compounds
            //dynamically using filters, which can be mixed with group compounds
            //filters are a great replacement to events to create subset of entities in a given state
            var damagedEntitiesfilter = _sveltoFilters
                   .GetTransientFilter<HealthComponent>(FilterIDs.damagedEntitiesFilter);

            ///This layer provide the "Damageable" tag and expect that damagable entities are found in a compound
            ///using this tag. Note that if I was expecting to have hundreds of entities, I would not have resorted
            ///to a complete iteration and if checks. Either I would have used another filter or used a Damaged
            ///tag compound 
            foreach (var ((entities, health, entityIDs, count), currentGroup) in entitiesDB
                            .QueryEntities<DamageableComponent, HealthComponent>(Damageable.Groups))
            {
                for (int i = 0; i < count; i++)
                {
                    //Add in the damagedEntitiesFilter all the entities that have been damaged this frame.
                    //I am using a transient filter. A transient filter is cleared at each submission so
                    //it is important that this engine runs before the filter is actually used
                    if (entities[i].damageInfo.damageToApply > 0)
                        damagedEntitiesfilter.Add(new EGID(entityIDs[i], currentGroup), (uint)i);
                }

                for (int i = 0; i < count; i++)
                {
                    health[i].currentHealth -= entities[i].damageInfo.damageToApply;
                    entities[i].damageInfo.damageToApply = 0; //reset instead to do a if may help with vectorization
                }
            }
        }

        public string name => nameof(ApplyDamageToDamageableEntitiesEngine);
        EntitiesDB.SveltoFilters _sveltoFilters;
    }
}