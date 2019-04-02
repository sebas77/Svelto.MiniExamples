using System;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAttackEngine : SingleEntityEngine<EnemyAttackEntityViewStruct>, IQueryingEntitiesEngine
    {
        public EnemyAttackEngine(ITime time)
        {
            _time        = time;
            _onCollidedWithTarget = OnCollidedWithTarget;
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { CheckIfHittingEnemyTarget().Run();}

        /// <summary>
        /// Add and Remove callback are enable by the SingleEntityEngine and MultiEntitiesEngine specifications
        /// They are called when:
        /// an Entity is built in a group
        /// an Entity is swapped in and from a group
        /// an Entity is removed from a group
        /// Be careful to handle the swap case separately from the other cases.
        /// </summary>
        /// <param name="entityViewStruct">the up to date entity</param>
        /// <param name="previousGroup">where the entity is coming from</param>
        protected override void Add(ref EnemyAttackEntityViewStruct entityViewStruct,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            //setup the Dispatch On Change only when the enemy is active
            if (entityViewStruct.ID.groupID == ECSGroups.ActiveEnemies)
            {
                entityViewStruct.targetTriggerComponent.hitChange =
                    DispatchExtensions.Setup(entityViewStruct.targetTriggerComponent.hitChange, entityViewStruct.ID);

                entityViewStruct.targetTriggerComponent.hitChange.NotifyOnValueSet(_onCollidedWithTarget);
            }
        }

        /// <summary>
        /// Add and Remove callback are enable by the SingleEntityEngine and MultiEntitiesEngine specifications
        /// They are called when:
        /// an Entity is built in a group
        /// an Entity is swapped in and from a group
        /// an Entity is removed from a group
        /// Be careful to handle the swap case separately from the other cases.
        /// </summary>
        /// <param name="entityViewStruct">the up to date entity</param>
        /// <param name="itsaSwap">if this Remove is caused by a swap</param>
        protected override void Remove(ref EnemyAttackEntityViewStruct entityViewStruct, bool itsaSwap)
        {
            if (entityViewStruct.ID.groupID == ECSGroups.ActiveEnemies)
                entityViewStruct.targetTriggerComponent.hitChange.StopNotify(_onCollidedWithTarget);
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
                        if (enemiesAttackData[enemyIndex].entityInRange.collides == true)
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
        
        readonly ITime _time;
        Action<EGID, EnemyCollisionData> _onCollidedWithTarget;
    }
}