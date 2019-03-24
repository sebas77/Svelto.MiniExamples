using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyMovementEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            Tick().Run();
        }

        IEnumerator Tick()
        {
            while (true)
            {
                int count;
                //query all the enemies from the standard group (no disabled nor respawning)
                var enemyTargetEntityViews = entitiesDB.QueryEntities<EnemyTargetEntityViewStruct>(ECSGroups.EnemyTargets, out count);

                if (count > 0)
                {
                    var enemies = entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out count);

                    for (var i = 0; i < count; i++)
                    {
                        enemies[i].movementComponent.navMeshDestination =
                            enemyTargetEntityViews[0].targetPositionComponent.position;
                    }
                }

                yield return null;
            }
        }
    }
}
