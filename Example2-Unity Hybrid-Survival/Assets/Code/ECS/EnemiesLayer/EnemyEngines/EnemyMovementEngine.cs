using System.Collections;
using Svelto.ECS.Example.Survive.Enemies.Svelto.ECS.Example.Survive.Enemies;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyMovementEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { _tick = Tick(); }

        IEnumerator Tick()
        {
            void RefHelper()
            {
                foreach (var ((enemiesTarget, _), _) in entitiesDB.QueryEntities<EnemyTargetEntityViewComponent>())
                {
                    //If there were more than one player, this must be smarter, for example choose the target
                    //according the distance
                    foreach (var ((enemies, enemiesCount), _) in entitiesDB.QueryEntities<EnemyEntityViewComponent>(
                        AliveEnemies.Groups))
                    {
                        //using always the first target because in this case I know there can be only one, but if 
                        //there were more, I could use different strategies, like choose the closest. This is 
                        //for a very simple AI scenario of course.
                        for (var i = 0; i < enemiesCount; i++)
                            enemies[i].movementComponent.navMeshDestination =
                                enemiesTarget[0].targetPositionComponent.position;
                    }
                }
            }

            while (true)
            {
                RefHelper();

                yield return null;
            }
        }

        public void   Step() { _tick.MoveNext(); }
        public string name   => nameof(EnemyMovementEngine);

        IEnumerator _tick;
    }
}