using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Transformable;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemyDeathEngine))]
    public class EnemyDeathEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnSwapEx<EnemyEntityViewComponent>
    {
        public EnemyDeathEngine(IEntityFunctions entityFunctions, IEntityStreamConsumerFactory consumerFactory,
            ITime time, WaitForSubmissionEnumerator waitForSubmission, GameObjectResourceManager manager)
        {
            _entityFunctions = entityFunctions;
            _consumerFactory = consumerFactory;
            _time = time;
            _waitForSubmission = waitForSubmission;
            _manager = manager;
            _animations = new FasterList<IEnumerator>();
            _consumer = _consumerFactory.GenerateConsumer<DeathComponent>("EnemyDeathEngine", 10);
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            while (_consumer.TryDequeue(out _, out EGID enemyID))
            {
                //publisher/consumer pattern will be replaces with better patterns in future for these cases.
                //The problem is obvious, DeathComponent is abstract and could have came from the player
                if (AliveEnemies.Includes(enemyID.groupID))
                    _animations.Add(PlayDeathSequence(enemyID));
            }

            for (uint i = 0; i < _animations.count; i++)
                if (_animations[i].MoveNext() == false)
                    _animations.UnorderedRemoveAt(i--);
        }

        public string name => nameof(EnemyDeathEngine);

        /// <summary>
        /// One of the available form of communication in Svelto.ECS: React On Swap allow to do what it says
        /// </summary>
        /// <param name="enemyView"></param>
        /// <param name="previousGroup"></param>
        /// <param name="egid"></param>
        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<EnemyEntityViewComponent> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            if (DeadEnemies.Includes(toGroup))
            {
                var (enemies, _) = entities;
                var (gos, _) = entitiesDB.QueryEntities<GameObjectEntityComponent>(toGroup);

                for (int i = (int)(rangeOfEntities.end - 1); i >= (int)rangeOfEntities.start; i--)
                {
                    enemies[i].movementComponent.navMeshEnabled = false;
                    enemies[i].movementComponent.setCapsuleAsTrigger = true;
                    gos[i].layer = GAME_LAYERS.NOT_SHOOTABLE_LAYER;
                }
            }
        }

        IEnumerator PlayDeathSequence(EGID egid)
        {
            var enemyView = entitiesDB.QueryEntity<EnemyEntityViewComponent>(egid);
            var enemyPos = entitiesDB.QueryEntity<PositionComponent>(egid);

            enemyView.animationComponent.playAnimation = "Dead";

            var goComponent = entitiesDB.QueryEntity<GameObjectEntityComponent>(egid);

            //Any build/swap/remove do not happen immediately, but at specific sync points
            //swapping group because we don't want any engine to pick up this entity while it's animating for death
            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(egid, DeadEnemies.BuildGroup);

            //wait for the swap to happen
            while (_waitForSubmission.MoveNext())
                yield return null;

            var wait = new WaitForSecondsEnumerator(2);

            while (wait.MoveNext())
            {
                enemyPos.position += -Vector3.up * 1.2f * _time.deltaTime;

                yield return null;
            }

            //new egid after the swap
            var entityGid = new EGID(egid.entityID, DeadEnemies.BuildGroup);
            var enemyType = entitiesDB.QueryEntity<EnemyComponent>(entityGid).enemyType;

            _manager.Recycle(goComponent.resourceIndex, (int)enemyType);
        }

        readonly IEntityFunctions _entityFunctions;
        readonly IEntityStreamConsumerFactory _consumerFactory;
        readonly ITime _time;
        Consumer<DeathComponent> _consumer;
        readonly WaitForSubmissionEnumerator _waitForSubmission;
        readonly GameObjectResourceManager _manager;
        readonly FasterList<IEnumerator> _animations;
    }
}