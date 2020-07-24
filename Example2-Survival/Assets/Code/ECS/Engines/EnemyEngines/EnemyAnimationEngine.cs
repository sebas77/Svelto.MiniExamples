using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAnimationEngine : IQueryingEntitiesEngine
    {
        public EnemyAnimationEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            _consumerHealth = _consumerFactory.GenerateConsumer<DamageableComponent>(ECSGroups.EnemiesGroup, "EnemyAnimationEngine", 15);
            _consumerDeath = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.PlayersGroup, "EnemyAnimationEngine", 1);
            
            SpawnEffectOnDamage().RunOnScheduler(StandardSchedulers.lateScheduler);
            CheckPlayerDeath().RunOnScheduler(StandardSchedulers.lateScheduler);
        }

        IEnumerator CheckPlayerDeath()
        {
            while (true)
            {
                while (_consumerDeath.TryDequeue(out _))
                {
                    //is player is dead, the enemy cheers
                    var (entity, count) = entitiesDB.QueryEntities<EnemyEntityViewComponent>(ECSGroups.EnemiesGroup);

                    for (var i = 0; i < count; i++)
                        entity[i].animationComponent.playAnimation = "PlayerDead";
                }

                yield return null;
            }
        }

        /// <summary>
        /// Lazyness alert: this shouldn't be in this engine
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnEffectOnDamage()
        {
            void CheckDamageEnemy(EGID egid, DamageableComponent component)
            {
                ref var enemyEntityViewsStructs = ref entitiesDB.QueryEntity<EnemyEntityViewComponent>(egid);

                enemyEntityViewsStructs.vfxComponent.position = component.damageInfo.damagePoint;
                enemyEntityViewsStructs.vfxComponent.play     = true;
            }

            while (true)
            {
                while (_consumerHealth.TryDequeue(out var component, out var egid))
                {
                    CheckDamageEnemy(egid, component);
                }

                yield return null;
            }
        }

        readonly IEntityStreamConsumerFactory _consumerFactory;
        Consumer<DamageableComponent> _consumerHealth;
        Consumer<DeathComponent> _consumerDeath;
    }
}