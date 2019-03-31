using System;
using System.Collections;
using Svelto.ECS.Example.Survive.Characters.Player;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAnimationEngine : IQueryingEntitiesEngine, IStep<PlayerDeathCondition>
    {
        readonly EnemyDeathSequencer _enemyDeadSequencer;
        readonly IEntityFunctions    _entityFunctions;

        readonly ITime _time;

        public EnemyAnimationEngine(ITime            time, EnemyDeathSequencer enemyDeadSequencer,
                                    IEntityFunctions entityFunctions)
        {
            _time               = time;
            _enemyDeadSequencer = enemyDeadSequencer;
            _entityFunctions    = entityFunctions;
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            AnimateOnDamage().Run();
            AnimateOnDeath().Run();
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            //is player is dead, the enemy cheers
            var entity = entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out var count);

            for (var i = 0; i < count; i++)
                entity[i].animationComponent.playAnimation = "PlayerDead";
        }

        IEnumerator AnimateOnDamage()
        {
            while (true)
            {
                var entities =
                    entitiesDB.QueryEntities<DamageableEntityStruct, EnemyEntityViewStruct>(ECSGroups.ActiveEnemies,
                                                                                            out var numberOfEnemies);

                var damageableEntityStructs = entities.Item1;
                var enemyEntityViewsStructs = entities.Item2;
                for (var i = 0; i < numberOfEnemies; i++)
                {
                    if (damageableEntityStructs[i].damaged == false) continue;

                    enemyEntityViewsStructs[i].vfxComponent.position =
                        damageableEntityStructs[i].damageInfo.damagePoint;
                    enemyEntityViewsStructs[i].vfxComponent.play = true;
                }

                yield return null;
            }
        }

        IEnumerator AnimateOnDeath()
        {
            while (true)
            {
                var entites =
                    entitiesDB.QueryEntities<EnemyEntityViewStruct, EnemySinkStruct, EnemyEntityStruct>(ECSGroups.DeadEnemiesGroups,
                                                                    out var numberOfEnemies);

                var enemyEntityViewsStructs = entites.Item1;
                var enemyEntitySinkStructs = entites.Item2;

                for (var i = 0; i < numberOfEnemies; i++)
                {
                    
                    var animationComponent = enemyEntityViewsStructs[i].animationComponent;
                    if (animationComponent.playAnimation != "Dead")
                    {
                        animationComponent.playAnimation        = "Dead";
                        enemyEntitySinkStructs[i].animationTime = DateTime.UtcNow.AddSeconds(2);
                    }
                    else
                    {
                        if (DateTime.UtcNow < enemyEntitySinkStructs[i].animationTime)
                        {
                            enemyEntityViewsStructs[i].transformComponent.position =
                                enemyEntityViewsStructs[i].positionComponent.position + -Vector3.up *
                                enemyEntitySinkStructs[i].sinkAnimSpeed * _time.deltaTime;
                        }
                        else
                        {
                            var enemyStructs = entites.Item3;
                                
                            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(enemyEntityViewsStructs[i].ID,
                                                                                    ECSGroups.EnemiesToRecycleGroups +
                                                                                    (uint) enemyStructs[i].enemyType);

                            _enemyDeadSequencer.Next(this, enemyEntityViewsStructs[i].ID);
                        }
                    }
                }

                yield return null;
            }
        }
    }
}