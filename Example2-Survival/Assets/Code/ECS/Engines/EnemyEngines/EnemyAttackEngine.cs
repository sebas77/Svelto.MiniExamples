using System;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAttackEngine
        : IReactOnAddAndRemove<EnemyAttackEntityViewStruct>, IReactOnSwap<EnemyAttackEntityViewStruct>,
          IQueryingEntitiesEngine
    {
        readonly Action<EGID, EnemyCollisionData> _onCollidedWithTarget;


        readonly ITime _time;

        public EnemyAttackEngine(ITime time)
        {
            _time                 = time;
            _onCollidedWithTarget = OnCollidedWithTarget;
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { CheckIfHittingEnemyTarget().Run(); }

        /// <summary>
        ///     Add and Remove callbacks are enabled by the IReactOnAddAndRemove interface
        ///     They are called when:
        ///     an Entity is built in a group  (no swap case)
        ///     an Entity is removed from a group (no swap case)
        /// </summary>
        /// <param name="entityViewStruct">the up to date entity</param>
        /// <param name="previousGroup">where the entity is coming from</param>
        public void Add(ref EnemyAttackEntityViewStruct entityViewStruct)
        {
            entityViewStruct.targetTriggerComponent.hitChange =
                new DispatchOnChange<EnemyCollisionData>(entityViewStruct.ID);

            entityViewStruct.targetTriggerComponent.hitChange.NotifyOnValueSet(_onCollidedWithTarget);
        }

        public void Remove(ref EnemyAttackEntityViewStruct entityViewStruct)
        {
            entityViewStruct.targetTriggerComponent.hitChange = null;
        }

        /// <summary>
        ///     NovedTo and MovedFrom callbacks are enabled by the IReactOnSwap interface
        ///     They are called on entity swap (when leaving a group and moving to the new one)
        /// </summary>
        /// <param name="entityViewStruct"></param>
        public void MovedFrom(ref EnemyAttackEntityViewStruct     entityViewStruct)
        {}

        public void MovedTo(ref EnemyAttackEntityViewStruct     entityViewStruct,
                            ExclusiveGroup.ExclusiveGroupStruct previousGroup)
        {
            if (entityViewStruct.ID.groupID == ECSGroups.ActiveEnemies)
                entityViewStruct.targetTriggerComponent.hitChange.ResumeNotify();
            else
                entityViewStruct.targetTriggerComponent.hitChange.PauseNotify();
        }

        void OnCollidedWithTarget(EGID sender, EnemyCollisionData enemyCollisionData)
        {
            entitiesDB.QueryEntity<EnemyAttackStruct>(sender).entityInRange = enemyCollisionData;
        }

        IEnumerator CheckIfHittingEnemyTarget()
        {
            while (true)
            {
                // The engine is querying the EnemyTargets group instead of the PlayersGroup.
                // this is more than a sophistication, it's the implementation of the rule that every engine must use
                // its own set of groups to promote encapsulation and modularity
                while (entitiesDB.HasAny<DamageableEntityStruct>(ECSGroups.EnemyTargets) == false ||
                       entitiesDB.HasAny<EnemyAttackEntityViewStruct>(ECSGroups.ActiveEnemies) == false)
                    yield return null;

                var targetEntities =
                    entitiesDB.QueryEntities<DamageableEntityStruct>(ECSGroups.EnemyTargets, out var targetsCount);
                var enemiesAttackData =
                    entitiesDB.QueryEntities<EnemyAttackStruct>(ECSGroups.ActiveEnemies, out var enemiesCount);

                //this is code show how you can use entity structs.
                //this case is banal, entity structs should be use to handle hundreds or thousands
                //of entities in a cache friendly and multi threaded code. However entity structs would allow
                //the creation of entity without any allocation, so they can be handy for
                //cases where entity should be built fast! Theoretically is possible to create
                //a game using only entity structs, but entity structs make sense ONLY if they
                //hold value types, so they come with a lot of limitations
                for (var enemyTargetIndex = 0; enemyTargetIndex < targetsCount; enemyTargetIndex++)
                {
                    for (var enemyIndex = 0; enemyIndex < enemiesCount; enemyIndex++)
                        if (enemiesAttackData[enemyIndex].entityInRange.collides)
                            if (enemiesAttackData[enemyIndex].entityInRange.otherEntityID ==
                                targetEntities[enemyTargetIndex].ID)
                            {
                                enemiesAttackData[enemyIndex].timer += _time.deltaTime;

                                if (enemiesAttackData[enemyIndex].timer >=
                                    enemiesAttackData[enemyIndex].timeBetweenAttack)
                                {
                                    enemiesAttackData[enemyIndex].timer = 0.0f;

                                    targetEntities[enemyTargetIndex].damageInfo =
                                        new DamageInfo(enemiesAttackData[enemyIndex].attackDamage, Vector3.zero);
                                }
                            }
                }

                yield return null;
            }
        }
    }
}