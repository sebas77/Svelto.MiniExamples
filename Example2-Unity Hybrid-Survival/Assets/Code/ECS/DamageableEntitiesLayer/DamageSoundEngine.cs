using UnityEngine;

namespace Svelto.ECS.Example.Survive.Damage
{
    public class DamageSoundEngine : IQueryingEntitiesEngine, IStepEngine, IReactOnAddAndRemove<DamageSoundEntityComponent>
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
        
        public void   Add(ref DamageSoundEntityComponent entityComponent, EGID egid)    { }

        //this assumes that removing an entity means it's death. If necessary I could check the group it comes from
        //to have a sort of transition callback on state change
        public void Remove(ref DamageSoundEntityComponent entityComponent, EGID egid)
        {
            entityComponent.playOneShot = AudioType.death;
        }

        void CheckForDamage()
        {
            while (_damageConsumer.TryDequeue(out _, out EGID ID))
            {
                entitiesDB.QueryEntity<DamageSoundEntityComponent>(ID).playOneShot =
                    AudioType.damage;
            }
        }

        Consumer<DamageableComponent> _damageConsumer;
    }
}