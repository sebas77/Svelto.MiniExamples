namespace Svelto.ECS.Example.Survive.Characters
{
    /// <summary>
    ///     The responsibility of this engine is to apply the damage to any damageable entity. If the logic applied to
    ///     the enemy was different compared to the logic applied to the player, I would have created two
    ///     different engines. Since the logic is the same, I can create an abstract engine for damageable entities
    ///     that doesn't need to know the type of specialized entity.
    ///     In my articles I introduce the concept of layered design, where several layers of abstractions can
    ///     co-exist. Every abstracted layer can be seen as a "framework" for the more specialized layers.
    ///     This would be part of an hypothetical "damageable entities" framework that could be distributed
    ///     independently by the specialised entities and reused in multiple projects.
    /// </summary>

    public class ApplyDamageToDamageableEntitiesEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public void Ready()
        {
            _consumer = _consumerFactory.GenerateConsumer<DamageableComponent>("DamageSoundEngine", 10);
        }

        public EntitiesDB entitiesDB { set; private get; }

        public ApplyDamageToDamageableEntitiesEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        public void Step()
        {
            while (_consumer.TryDequeue(out var damageableComponent, out EGID ID))
            {
                ref var entityHealth = ref entitiesDB.QueryEntity<HealthComponent>(ID);

                entityHealth.currentHealth -= damageableComponent.damageInfo.damageToApply;
                
                entitiesDB.PublishEntityChange<HealthComponent>(ID);
            }
        }

        public string name => nameof(ApplyDamageToDamageableEntitiesEngine);
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
        Consumer<DamageableComponent> _consumer;
    }
}