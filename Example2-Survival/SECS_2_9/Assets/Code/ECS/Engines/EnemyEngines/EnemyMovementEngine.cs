using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyMovementEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { Tick().Run(); }

        IEnumerator Tick()
        {
            while (true)
            {
                //query all the enemies from the standard group (no disabled nor respawning)
                var enemyTargetEntityViews =
                    entitiesDB.QueryEntities<EnemyTargetEntityViewStruct>(
                        ECSGroups.EnemyTargets, out var enemyTargetsCount);

                if (enemyTargetsCount > 0)
                {
                    var enemies =
                        entitiesDB.QueryEntities<EnemyEntityViewStruct>(ECSGroups.ActiveEnemies, out var enemiesCount);

                    //using always the first target because in this case I know there can be only one, but if 
                    //there were more, I could use different strategies, like choose the closest. This is 
                    //for a very simple AI scenario of course.
                    for (var i = 0; i < enemiesCount; i++)
                        enemies[i].movementComponent.navMeshDestination =
                            enemyTargetEntityViews[0].targetPositionComponent.position;
                }

                yield return null;
            }
        }
    }
}