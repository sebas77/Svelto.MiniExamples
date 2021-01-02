using System.Collections;
using Svelto.Common;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage))]
    public class EnemySpawnEffectOnDamage: IQueryingEntitiesEngine, IStepEngine
    {
        public EnemySpawnEffectOnDamage(IEntityStreamConsumerFactory consumerFactory)
        {
            //this consumer will process only changes from DamageableComponent published from the EnemiesGroup
            _consumerHealth = consumerFactory.GenerateConsumer<DamageableComponent>(ECSGroups.EnemiesGroup, "EnemyAnimationEngine", 15);
            _checkForEnemyDamage = SpawnEffectOnDamage();
        }

        public void Step()
        {
            _checkForEnemyDamage.MoveNext();
        }
        public string name => nameof(EnemySpawnEffectOnDamage);

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { }

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

        readonly IEnumerator          _checkForEnemyDamage;
        Consumer<DamageableComponent> _consumerHealth;
    }
}