using System;
using System.Collections;
using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAnimationEngine : IQueryingEntitiesEngine
                                      , IStep<PlayerDeathCondition>
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public EnemyAnimationEngine(ITime time, EnemyDeathSequencer enemyDeadSequencer, IEntityFunctions entityFunctions)
        {
            _time = time;
            _enemyDeadSequencer = enemyDeadSequencer;
            _entityFunctions = entityFunctions;
        }

        public void Ready()
        {
            AnimateOnDamage().Run();
            AnimateOnDeath().Run();
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            //is player is dead, the enemy cheers
            int count;
            var entity = entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out count);

            for (var i = 0; i < count; i++)
                entity[i].animationComponent.playAnimation = "PlayerDead";
        }
        
        IEnumerator AnimateOnDamage()
        {
            while (true)
            {
                int numberOfEnemies;
                var damageableEntityStructs =
                    entitiesDB.QueryEntities<DamageableEntityStruct>(ECSGroups.ActiveEnemies, out numberOfEnemies);
                var enemyEntityViewsStructs =
                    entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out numberOfEnemies);

                for (int i = 0; i < numberOfEnemies; i++)
                {
                    if (damageableEntityStructs[i].damaged == false) continue;

                    enemyEntityViewsStructs[i].vfxComponent.position = damageableEntityStructs[i].damageInfo.damagePoint;
                    enemyEntityViewsStructs[i].vfxComponent.play = true;
                }

                yield return null;
            }
        }
        
        IEnumerator AnimateOnDeath()
        {
            while (true)
            {
                int numberOfEnemies;
                var enemyEntityViewsStructs =
                    entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.DeadEnemiesGroups, out numberOfEnemies);
                var enemyEntitySinkStructs =
                    entitiesDB.QueryEntities<EnemySinkStruct>(ECSGroups.DeadEnemiesGroups, out numberOfEnemies);
            
                for (int i = 0; i < numberOfEnemies; i++)
                {
                    var animationComponent = enemyEntityViewsStructs[i].animationComponent;
                    if (animationComponent.playAnimation != "Dead")
                    {
                        animationComponent.playAnimation = "Dead";
                        enemyEntitySinkStructs[i].animationTime = DateTime.UtcNow.AddSeconds(2);
                    }
                    else
                    {
                        if (DateTime.UtcNow < enemyEntitySinkStructs[i].animationTime)
                        {
                            enemyEntityViewsStructs[i].transformComponent.position = 
                                enemyEntityViewsStructs[i].positionComponent.position + -UnityEngine.Vector3.up * enemyEntitySinkStructs[i].sinkAnimSpeed * _time.deltaTime;
                        }
                        else
                        {
                            var enemyStructs =
                                entitiesDB.QueryEntities<EnemyEntityStruct>(
                                    ECSGroups.DeadEnemiesGroups, out numberOfEnemies);
                            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(
                                enemyEntityViewsStructs[i].ID,
                                ECSGroups.EnemiesToRecycleGroups + (int) enemyStructs[i].enemyType);

                            _enemyDeadSequencer.Next(this, enemyEntityViewsStructs[i].ID);
                        }
                    }
                }

                yield return null;
            }
        }

        readonly ITime               _time;
        readonly EnemyDeathSequencer _enemyDeadSequencer;
        readonly IEntityFunctions    _entityFunctions;
    }
}