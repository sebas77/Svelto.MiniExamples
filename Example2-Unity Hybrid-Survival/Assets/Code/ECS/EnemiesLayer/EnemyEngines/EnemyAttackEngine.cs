using System;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemyAttackEngine))]
    public class EnemyAttackEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EnemyAttackEngine(ITime time)
        {
            _time                 = time;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() {}

        public void Step()
        {
            //get all the entities with EnemyAttack and Collision Components that are in any Enemies Group
            foreach (var ((enemiesAttackData, collision, enemiesCount), _) in entitiesDB
                            .QueryEntities<EnemyAttackComponent, CollisionComponent>(
                                 EnemyAliveGroup.Groups))
            {
                for (var enemyIndex = enemiesCount - 1; enemyIndex >= 0; enemyIndex--)
                {
                    ref var enemyAttackComponent = ref enemiesAttackData[enemyIndex];
                    ref CollisionData collisionData   = ref collision[enemyIndex].entityInRange;

                    //a collision was previously registered
                    if (collisionData.collides == true)
                    {
                        enemyAttackComponent.timer += _time.deltaTime;

                        if (enemyAttackComponent.timer >= enemyAttackComponent.timeBetweenAttack)
                        {
                            enemyAttackComponent.timer = 0.0f;

                            //if this fails, it means that the target entity doesn't exist anymore. 
                            //Unluckily Unity doesn't trigger OnTriggerExit on RB disabled and any way to find
                            //a work around it, would have been much more complex than just this if
                            if (collisionData.otherEntityID.ToEGID(entitiesDB, out var otherEntityID) == true)
                            {
                                DamageTargetInsideRange(otherEntityID, enemyAttackComponent.attackDamage);
                            }
                        }
                    }
                }
            }
        }

        void DamageTargetInsideRange(in EGID otherEntityID, int attackDamage)
        {
            entitiesDB.QueryEntity<DamageableComponent>(otherEntityID).damageInfo =
                new DamageInfo(attackDamage, Vector3.zero);
        }

        public string name => nameof(EnemyAttackEngine);

        readonly ITime                            _time;
    }
}