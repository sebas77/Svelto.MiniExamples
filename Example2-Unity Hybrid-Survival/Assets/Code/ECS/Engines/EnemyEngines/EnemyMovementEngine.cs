using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyMovementEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { _tick = Tick(); }

        IEnumerator Tick()
        {
            while (true)
            {
                while (entitiesDB.HasAny<EnemyTargetEntityViewComponent>(ECSGroups.EnemiesTargetGroup) == false)
                    yield return null;

                //there is only one enemy target in this demo. If there were multiple, this code would look
                //very different
                var enemyTarget =
                    entitiesDB.QueryUniqueEntity<EnemyTargetEntityViewComponent>(ECSGroups.EnemiesTargetGroup);

                var (enemies, enemiesCount) =
                    entitiesDB.QueryEntities<EnemyEntityViewComponent>(ECSGroups.EnemiesGroup);

                //using always the first target because in this case I know there can be only one, but if 
                //there were more, I could use different strategies, like choose the closest. This is 
                //for a very simple AI scenario of course.
                for (var i = 0; i < enemiesCount; i++)
                    enemies[i].movementComponent.navMeshDestination = enemyTarget.targetPositionComponent.position;

                yield return null;
            }
        }

        public void   Step() { _tick.MoveNext(); }
        public string name   => nameof(EnemyMovementEngine);

        IEnumerator _tick;
    }
}