using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemyDeathEngine))]
    public class EnemyDeathEngine : IQueryingEntitiesEngine, IStepEngine, IReactOnSwap<EnemyEntityViewComponent>
    {
        public EnemyDeathEngine(IEntityFunctions entityFunctions, IEntityStreamConsumerFactory consumerFactory, 
                                ITime time, WaitForSubmissionEnumerator waitForSubmission)
        {
            _entityFunctions   = entityFunctions;
            _consumerFactory   = consumerFactory;
            _time              = time;
            _waitForSubmission = waitForSubmission;
            _animations        = new FasterList<IEnumerator>();
            _consumer = _consumerFactory.GenerateConsumer<DeathComponent>(
                ECSGroups.EnemiesGroup, "EnemyDeathEngine", 10);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            while (_consumer.TryDequeue(out _, out EGID egid))
            {
                _animations.Add(StartSeparateAnimation(egid));
            }
            
            for (int i = 0; i < _animations.count; i++)
                if (_animations[i].MoveNext() == false)
                    _animations.UnorderedRemoveAt(i--);
        }
        public string name   => nameof(EnemyDeathEngine);

        /// <summary>
        /// One of the available form of communication in Svelto.ECS: React On Swap allow to do what it says
        /// </summary>
        /// <param name="enemyView"></param>
        /// <param name="previousGroup"></param>
        /// <param name="egid"></param>
        public void MovedTo(ref EnemyEntityViewComponent enemyView, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (egid.groupID == ECSGroups.EnemiesDeadGroup)
            {
                enemyView.layerComponent.layer                  = GAME_LAYERS.NOT_SHOOTABLE_MASK;
                enemyView.movementComponent.navMeshEnabled      = false;
                enemyView.movementComponent.setCapsuleAsTrigger = true;
            }
        }

        IEnumerator StartSeparateAnimation(EGID egid)
        {
            var enemyView = entitiesDB.QueryEntity<EnemyEntityViewComponent>(egid);

            enemyView.animationComponent.playAnimation = "Dead";

            //Any build/swap/remove do not happen immediately, but at specific sync points
            //swapping group because we don't want any engine to pick up this entity while it's animating for death
            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(egid, ECSGroups.EnemiesDeadGroup);

            //wait for the swap to happen
            while (_waitForSubmission.MoveNext())
                yield return null;

            var wait = new WaitForSecondsEnumerator(2);

            while (wait.MoveNext())
            {
                enemyView.transformComponent.position =
                    enemyView.positionComponent.position + -Vector3.up * 1.2f * _time.deltaTime;

                yield return null;
            }

            //new egid after the swap
            var entityGid = new EGID(egid.entityID, ECSGroups.EnemiesDeadGroup);

            var enemyType = entitiesDB.QueryEntity<EnemyComponent>(entityGid).enemyType;

            //getting ready to recycle it
            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(
                entityGid, ECSGroups.EnemiesToRecycleGroups + (uint) enemyType);
        }

        readonly IEntityFunctions             _entityFunctions;
        readonly IEntityStreamConsumerFactory _consumerFactory;
        readonly ITime                        _time;

        Consumer<DeathComponent>         _consumer;
        WaitForSubmissionEnumerator      _waitForSubmission;
        readonly FasterList<IEnumerator> _animations;
    }
}