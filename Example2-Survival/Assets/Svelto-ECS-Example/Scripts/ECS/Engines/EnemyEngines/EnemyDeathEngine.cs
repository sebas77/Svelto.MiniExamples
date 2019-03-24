using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyDeathEngine : IQueryingEntitiesEngine
    {
        public EnemyDeathEngine(IEntityFunctions entityFunctions, EnemyDeathSequencer enemyDeadSequencer)
        {
            _entityFunctions = entityFunctions;

            _enemyDeadSequencer = enemyDeadSequencer;
        }

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            CheckIfDead().Run();
        }

        IEnumerator CheckIfDead()
        {
            while (true)
            {
                //wait for enemies to be created
                while (entitiesDB.HasAny<EnemyEntityStruct>(ECSGroups.ActiveEnemies) == false) yield return null;

                //Groups affect the memory layour. Entity views are split according groups, so that even if entity
                //views are used by entities outside a specific group, those entity views won't be present 
                //in the array returned by QueryEntities.
                int count;
                var enemyEntitiesViews =
                    entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out count);
                var enemyEntitiesHealth =
                    entitiesDB.QueryEntities<HealthEntityStruct>(ECSGroups.ActiveEnemies, out count);

                for (int index = 0; index < count; index++)
                {
                    if (enemyEntitiesHealth[index].dead == false) continue;

                    SetParametersForDeath(ref enemyEntitiesViews[index]);
                    
                    _enemyDeadSequencer.Next(this, enemyEntitiesViews[index].ID);

                    _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(enemyEntitiesViews[index].ID, ECSGroups.DeadEnemiesGroups);
                }

                yield return null;
            }
        }

        static void SetParametersForDeath(ref EnemyEntityViewStruct enemyEntityViewStruct)
        {
            enemyEntityViewStruct.layerComponent.layer                  = GAME_LAYERS.NOT_SHOOTABLE_MASK;
            enemyEntityViewStruct.movementComponent.navMeshEnabled      = false;
            enemyEntityViewStruct.movementComponent.setCapsuleAsTrigger = true;
        }

        readonly IEntityFunctions    _entityFunctions;
        readonly EnemyDeathSequencer _enemyDeadSequencer;
    }
}
