using System;
using Svelto.Common;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    [Sequenced(nameof(EnginesEnum.EnemyAttackEngine))]
    public class EnemyAttackEngine : IReactOnAddAndRemove<EnemyAttackEntityViewComponent>
                                   , IReactOnSwap<EnemyAttackEntityViewComponent>, IQueryingEntitiesEngine, IStepEngine
    {
        public EnemyAttackEngine(ITime time)
        {
            _time                 = time;
            _onCollidedWithTarget = OnCollidedWithTarget;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() {}

        /// <summary>
        ///     Add and Remove callbacks are enabled by the IReactOnAddAndRemove interface
        ///     They are called when:
        ///     an Entity is built in a group  (no swap case)
        ///     an Entity is removed from a group (no swap case)
        /// </summary>
        /// <param name="entityViewComponent">the up to date entity</param>
        /// <param name="previousGroup">where the entity is coming from</param>
        public void Add(ref EnemyAttackEntityViewComponent entityViewComponent, EGID egid)
        {
            var dispatchOnChange = new DispatchOnChange<EnemyCollisionData>(egid);

            dispatchOnChange.NotifyOnValueSet(_onCollidedWithTarget);

            entityViewComponent.targetTriggerComponent.hitChange = dispatchOnChange;
        }

        public void Remove(ref EnemyAttackEntityViewComponent entityViewComponent, EGID egid)
        {
            entityViewComponent.targetTriggerComponent.hitChange = null;
        }

        /// <summary>
        ///     MovedTo callbacks are enabled by the IReactOnSwap interface
        ///     They are called on entity swap (when leaving a group and moving to the new one)
        /// </summary>
        /// <param name="entityViewComponent"></param>
        public void MovedTo
            (ref EnemyAttackEntityViewComponent entityViewComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            if (egid.groupID == ECSGroups.EnemiesGroup)
                entityViewComponent.targetTriggerComponent.hitChange.ResumeNotify();
            else
                entityViewComponent.targetTriggerComponent.hitChange.PauseNotify();
        }

        /// <summary>
        /// once an enemy enters in a trigger, we set the trigger data built inside the implementor and sent
        /// through the DispatchOnChange
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="enemyCollisionData"></param>
        void OnCollidedWithTarget(EGID sender, EnemyCollisionData enemyCollisionData)
        {
            entitiesDB.QueryEntity<EnemyAttackComponent>(sender).entityInRange = enemyCollisionData;
        }

        public void Step()
        {
            // The engine is querying the EnemyTargets group instead of the PlayersGroup.
            // this is more than a sophistication, it's the implementation of the rule that every engine must use
            // its own set of groups to promote encapsulation and modularity
            var (targetEntities, targetsCount) =
                entitiesDB.QueryEntities<DamageableComponent>(ECSGroups.EnemiesTargetGroup);

            if (targetsCount == 0)
                return;
                
            var (enemiesAttackData, enemiesCount) =
                entitiesDB.QueryEntities<EnemyAttackComponent>(ECSGroups.EnemiesGroup);
            
            if (enemiesCount == 0)
                return;
            
            //this is code show how you can use entity structs.
            //this case is banal, entity structs should be use to handle hundreds or thousands
            //of entities in a cache friendly and multi threaded code. However entity structs would allow
            //the creation of entity without any allocation, so they can be handy for
            //cases where entity should be built fast! Theoretically is possible to create
            //a game using only entity structs if the framework allows so
            for (var enemyTargetIndex = 0; enemyTargetIndex < targetsCount; enemyTargetIndex++)
            {
                for (var enemyIndex = 0; enemyIndex < enemiesCount; enemyIndex++)
                {
                    ref var enemyAttackComponent = ref enemiesAttackData[enemyIndex];
                    ref var enemyCollisionData   = ref enemyAttackComponent.entityInRange;

                    if (enemyCollisionData.collides
                     && enemyCollisionData.otherEntityID == targetEntities[enemyTargetIndex].ID)
                    {
                        enemyAttackComponent.timer += _time.deltaTime;

                        if (enemyAttackComponent.timer >= enemyAttackComponent.timeBetweenAttack)
                        {
                            enemyAttackComponent.timer = 0.0f;

                            targetEntities[enemyTargetIndex].damageInfo =
                                new DamageInfo(enemyAttackComponent.attackDamage, Vector3.zero);
                                
                            entitiesDB.PublishEntityChange<DamageableComponent>(targetEntities[enemyTargetIndex].ID);
                        }
                    }
                }
            }
        }

        public string name => nameof(EnemyAttackEngine);

        readonly Action<EGID, EnemyCollisionData> _onCollidedWithTarget;
        readonly ITime                            _time;
    }
}