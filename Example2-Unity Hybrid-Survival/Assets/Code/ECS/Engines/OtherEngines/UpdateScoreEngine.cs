using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Characters;

namespace Svelto.ECS.Example.Survive.HUD
{
    [Sequenced(nameof(EnginesNames.UpdateScoreEngine))]
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
            var consumer = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.EnemiesGroup, "ScoreEngine", 1);

            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out var egid))
                {
                    var playerTargets = entitiesDB.QueryEntitiesAndIndex<ScoreValueComponent>(egid, out var index);

                    hudEntityView.scoreComponent.score += playerTargets[index].scoreValue;
                }

                yield return null;
            }
        }

        public UpdateScoreEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
        IEnumerator                           _listenForEnemyDeath;
    }
}