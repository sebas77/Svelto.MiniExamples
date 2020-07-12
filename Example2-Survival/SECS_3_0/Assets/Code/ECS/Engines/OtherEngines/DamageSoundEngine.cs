using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Sounds
{
    public class DamageSoundEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            CheckForDamage().RunOnScheduler(StandardSchedulers.lateScheduler);
            CheckForDeath().RunOnScheduler(StandardSchedulers.lateScheduler);
        }

        IEnumerator CheckForDeath()
        {
            var consumer = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.PlayersGroup, "DamageSoundEngine", 1);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out EGID ID))
                {
                    entitiesDB.QueryEntity<DamageSoundEntityView>(ID).audioComponent.playOneShot = AudioType.death;
                }

                yield return null;
            }
        }

        IEnumerator CheckForDamage()
        {
            var consumer = _consumerFactory.GenerateConsumer<DamageableComponent>(ECSGroups.PlayersGroup, "DamageSoundEngine", 1);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out EGID ID))
                {
                    entitiesDB.QueryEntity<DamageSoundEntityView>(ID).audioComponent.playOneShot = AudioType.damage;
                }
        
                yield return null;
            }
        }

        public DamageSoundEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        readonly IEntityStreamConsumerFactory _consumerFactory;
    }
}