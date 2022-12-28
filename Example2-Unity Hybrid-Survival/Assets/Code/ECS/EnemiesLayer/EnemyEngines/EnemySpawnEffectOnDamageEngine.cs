using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using AudioType = Svelto.ECS.Example.Survive.Damage.AudioType;

namespace Svelto.ECS.Example.Survive.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage))]
    public class EnemySpawnEffectOnDamageEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public EnemySpawnEffectOnDamageEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            //this consumer will process only changes from DamageableComponent published from the EnemiesGroup
            _checkForEnemyDamage = SpawnEffectOnDamage();
        }

        public void Step()
        {
            _checkForEnemyDamage.MoveNext();
        }

        public string name => nameof(EnemySpawnEffectOnDamageEngine);

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            _sveltoFilters = entitiesDB.GetFilters();
        }

        IEnumerator SpawnEffectOnDamage()
        {
            while (true)
            {
                RefHelper();

                yield return null;
            }

            void RefHelper()
            {
                var deadEntitiesFilter =
                        _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.damagedEntitiesFilter);

                foreach (var (filteredIndices, group) in deadEntitiesFilter)
                {
                    if (EnemiesGroup.Includes(group)) //is it an enemy?
                    {
                        var (damage, vfx, sound, _) =
                                entitiesDB.QueryEntities<DamageableComponent, VFXComponent, DamageSoundComponent>(
                                    group);

                        var indicesCount = filteredIndices.count;
                        for (int i = 0; i < indicesCount; i++)
                        {
                            var filteredIndex = filteredIndices[i];
                            
                            sound[filteredIndex].playOneShot = (int)AudioType.damage;

                            vfx[filteredIndex].vfxEvent = new VFXEvent(damage[filteredIndex].damageInfo.damagePoint);
                        }
                    }
                }
            }
        }

        readonly IEnumerator _checkForEnemyDamage;
        EntitiesDB.SveltoFilters _sveltoFilters;
    }
}