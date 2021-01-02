namespace Svelto.ECS.Example.Survive.Characters.Sounds
{
    public class DamageSoundEngine : IQueryingEntitiesEngine, IStepEngine, IReactOnAddAndRemove<DamageSoundEntityViewComponent>
    {
        public DamageSoundEngine(IEntityStreamConsumerFactory consumerFactory)
        {
           _damageConsumer = consumerFactory.GenerateConsumer<DamageableComponent>("DamageSoundEngine", 1);
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        { }

        public void Step()
        {
            CheckForDamage();
        }

        public string name => nameof(DamageSoundEngine);
        
        public void   Add(ref DamageSoundEntityViewComponent entityComponent, EGID egid)    { }

        //this assumes that removing an entity means it's death. If necessary I could check the group it comes from
        //to have a sort of transition callback on state change
        public void Remove(ref DamageSoundEntityViewComponent entityComponent, EGID egid)
        {
            entityComponent.audioComponent.playOneShot = AudioType.death;
        }

        void CheckForDamage()
        {
            while (_damageConsumer.TryDequeue(out _, out EGID ID))
            {
                entitiesDB.QueryEntity<DamageSoundEntityViewComponent>(ID).audioComponent.playOneShot =
                    AudioType.damage;
            }
        }

        Consumer<DamageableComponent> _damageConsumer;
    }
}