using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyDeathEngine : IQueryingEntitiesEngine
    {
        readonly EnemyDeathSequencer _enemyDeadSequencer;

        readonly IEntityFunctions _entityFunctions;

        public EnemyDeathEngine(IEntityFunctions entityFunctions, EnemyDeathSequencer enemyDeadSequencer)
        {
            _entityFunctions = entityFunctions;

            _enemyDeadSequencer = enemyDeadSequencer;
        }

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { CheckIfDead().Run(); }

        IEnumerator CheckIfDead()
        {
            while (true)
            {
                //wait for enemies to be created
                while (entitiesDB.HasAny<EnemyEntityStruct>(ECSGroups.ActiveEnemies) == false) yield return null;

                //Groups affect the memory layour. Entity views are split according groups, so that even if entity
                //views are used by entities outside a specific group, those entity views won't be present 
                //in the array returned by QueryEntities.
                var entities =
                    entitiesDB.QueryEntities<EnemyEntityViewStruct, HealthEntityStruct, EnemyAttackStruct>(ECSGroups.ActiveEnemies, out var count);

                var enemyEntitiesHealth = entities.Item2; 
                var enemyEntitiesViews = entities.Item1;
                var enemyAttackStruct = entities.Item3;
                
                for (var index = 0; index < count; index++)
                {
                    if (enemyEntitiesHealth[index].dead == false) continue;

                    SetParametersForDeath(ref enemyEntitiesViews[index], ref enemyAttackStruct[index]);

                    _enemyDeadSequencer.Next(this, enemyEntitiesViews[index].ID);
                    _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(enemyEntitiesViews[index].ID,
                                                                            ECSGroups.DeadEnemiesGroups);
                }

                yield return null;
            }
        }

        static void SetParametersForDeath(ref EnemyEntityViewStruct enemyEntityViewStruct,
                                          ref EnemyAttackStruct     enemyAttackStruct)
        {
            enemyEntityViewStruct.layerComponent.layer                  = GAME_LAYERS.NOT_SHOOTABLE_MASK;
            enemyEntityViewStruct.movementComponent.navMeshEnabled      = false;
//            enemyEntityViewStruct.movementComponent.setCapsuleAsTrigger = true;
            enemyAttackStruct.entityInRange = new EnemyCollisionData();
        }
    }
}