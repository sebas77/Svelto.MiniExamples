using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;

namespace Svelto.ECS.Example.Survive.HUD
{
    [Sequenced(nameof(HUDEnginesNames.UpdateScoreEngine))]
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
            var consumer = _consumerFactory.GenerateConsumer<DeathComponent>("ScoreEngine", 1);

            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out var egid))
                {
                    //todo: this would be considered a design mistake, I will need to fix this with future refactoring
                    //this engine cannot be aware of the concept of Enemy. It shouldn't be aware of the concept
                    //of Death either. The scoring system and what cause the score to change must be decoupled
                    //on the other hand, it would be also wrong to let the death/enemy engines be aware of what
                    //event causes a score change.
                    if (egid.groupID.FoundIn(AliveEnemies.Groups))
                    {
                        var playerTargets = entitiesDB.QueryEntitiesAndIndex<ScoreValueComponent>(egid, out var index);

                        hudEntityView.scoreComponent.score += playerTargets[index].scoreValue;
                    }
                }

                yield return null;
            }
        }

        public UpdateScoreEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
        IEnumerator                           _listenForEnemyDeath;
    }
}