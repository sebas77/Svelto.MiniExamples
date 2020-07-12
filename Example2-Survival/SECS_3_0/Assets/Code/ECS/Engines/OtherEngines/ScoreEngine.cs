using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.HUD
{
    public class ScoreEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            ListenForEnemyDeath().RunOnScheduler(StandardSchedulers.lateScheduler);
        }

        public IEnumerator ListenForEnemyDeath()
        {
            var consumer = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.EnemiesGroup, "ScoreEngine", 1);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out var egid))
                {
                    var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityView>(ECSGroups.GUICanvas);
                    var playerTargets = entitiesDB.QueryEntitiesAndIndex<ScoreValueComponent>(egid, out var index);

                    hudEntityView.scoreComponent.score += playerTargets[index].scoreValue;
                }

                yield return null;
            }
        }

        public ScoreEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
    }
}