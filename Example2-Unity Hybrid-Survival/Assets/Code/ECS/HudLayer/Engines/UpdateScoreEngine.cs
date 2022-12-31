using System.Collections;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;

namespace Svelto.ECS.Example.Survive.HUD
{
    public class UpdateScoreEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _listenForEnemyDeath = ListenForEnemyDeath();
        }
        
        public void   Step() { _listenForEnemyDeath.MoveNext(); }
        public string name   => nameof(UpdateScoreEngine);

        IEnumerator ListenForEnemyDeath()
        {
            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;
            
            var _sveltoFilters = entitiesDB.GetFilters();
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            
            while (true)
            {
                RefHelper();

                yield return null;
            }
            
            void RefHelper()
            {
                var deaddEntitiesFilter =
                        _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.DeadEntitiesFilter);

                //iterate the subset of entities that are killed on this frame
                foreach (var (filteredIndices, group) in deaddEntitiesFilter)
                {
                    if (EnemyAliveGroup.Includes(group)) //is it an enemy?
                    {
                        var (score, _) = entitiesDB.QueryEntities<ScoreValueComponent>(group);

                        var indicesCount = filteredIndices.count;
                        for (int i = 0; i < indicesCount; i++)
                        {
                            var filteredIndex = filteredIndices[i];
                            hudEntityView.scoreComponent.score += score[filteredIndex].scoreValue;
                        }
                    }
                }
            }
        }

        IEnumerator                           _listenForEnemyDeath;
    }
}