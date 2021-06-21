namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyChangeAnimationOnPlayerDeathEngine : IReactOnAddAndRemove<EnemyTargetEntityViewComponent>, IQueryingEntitiesEngine
    {
        public void Add(ref EnemyTargetEntityViewComponent entityComponent, EGID egid) { }

        public void Remove(ref EnemyTargetEntityViewComponent entityComponent, EGID egid)
        {
            foreach (var ((enemies, count), _) in entitiesDB.QueryEntities<EnemyEntityViewComponent>(Enemies.AliveEnemies.Groups))
            {
                for (int i = 0; i < count; i++)
                {
                    enemies[i].animationComponent.playAnimation = "PlayerDead";
                    enemies[i].movementComponent.navMeshEnabled = false;
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
    }
}