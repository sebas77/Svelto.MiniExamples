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
        public EnemySpawnEffectOnDamageEngine()
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
                var damagedEntitiesFilter =
                        _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.DamagedEntitiesFilter);

                //iterate the subset of entities that are damaged
                foreach (var (filteredIndices, group) in damagedEntitiesFilter)
                {
                    if (EnemyAliveGroup.Includes(group)) //is it an enemy?
                    {
                        var (damage, vfx, sound, _) =
                                entitiesDB.QueryEntities<DamageableComponent, VFXComponent, SoundComponent>(
                                    group);

                        var indicesCount = filteredIndices.count;
                        for (int i = 0; i < indicesCount; i++)
                        {
                            //remember: filters work with double indexing
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