using System.Collections;
using Svelto.Common;
using Svelto.ECS.Extensions;
using Svelto.Tasks.Enumerators;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    [Sequenced(nameof(EnginesEnum.EnemyDeathEngine))]
    public class EnemyDeathEngine : IQueryingEntitiesEngine, IStepEngine, IReactOnSwap<EnemyEntityViewComponent>
    {
        public EnemyDeathEngine
            (IEntityFunctions entityFunctions, IEntityStreamConsumerFactory consumerFactory, IEntityFactory entityFactory, ITime time)
        {
            _entityFunctions = entityFunctions;
            _consumerFactory = consumerFactory;
            _time            = time;
            _entityFactory = entityFactory;
            _checkIfDead     = CheckIfDead();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _consumer = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.EnemiesGroup, "EnemyDeathEngine", 10);
        }

        public void   Step() { _checkIfDead.MoveNext(); }
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

        IEnumerator CheckIfDead()
        {
            while (true)
            {
                while (_consumer.TryDequeue(out _, out EGID id))
                {
                    KillEnemySequence(id).Run();
                }

                yield return null;
            }
        }

        IEnumerator KillEnemySequence(EGID egid)
        {
            void InitialSetup()
            {
                ref var enemyView = ref entitiesDB.QueryEntity<EnemyEntityViewComponent>(egid);

                enemyView.animationComponent.playAnimation = "Dead";

                //Any build/swap/remove do not happen immediately, but at specific sync points
                //swapping group because we don't want any engine to pick up this entity while it's animating for death
                _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(egid, ECSGroups.EnemiesDeadGroup);
            }

            InitialSetup();

            //wait for the swap to happen
            yield return new WaitForSubmissionEnumerator(_entityFunctions, _entityFactory, entitiesDB);

            var wait = new WaitForSecondsEnumerator(2);
            
            //new egid after the swap
            var entityGid = new EGID(egid.entityID, ECSGroups.EnemiesDeadGroup);

            while (wait.MoveNext())
            {
                var enemyView = entitiesDB.QueryEntity<EnemyEntityViewComponent>(entityGid);
                
                enemyView.transformComponent.position =
                    enemyView.positionComponent.position + -Vector3.up * 1.2f * _time.deltaTime;

                yield return null;
            }

            var enemyType =
                entitiesDB.QueryEntity<EnemyComponent>(entityGid).enemyType;

            //getting ready to recycle it
            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(
                entityGid, ECSGroups.EnemiesToRecycleGroups + (uint) enemyType);
        }

        readonly IEntityFunctions             _entityFunctions;
        readonly IEntityStreamConsumerFactory _consumerFactory;
        readonly IEnumerator                  _checkIfDead;
        readonly ITime                        _time;
        readonly IEntityFactory _entityFactory;
        Consumer<DeathComponent> _consumer;
    }
}