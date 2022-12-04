//namespace Svelto.ECS.Example.Survive.Enemies
//{
//    public class EnemyChangeAnimationOnPlayerDeathEngine : IReactOnAddAndRemove<EnemyTargetEntityComponent>, IQueryingEntitiesEngine
//    {
//        public void Add(ref EnemyTargetEntityComponent entityComponent, EGID egid) { }
//
//        public void Remove(ref EnemyTargetEntityComponent entityComponent, EGID egid)
//        {
//            foreach (var ((enemies, count), _) in entitiesDB.QueryEntities<EnemyEntityViewComponent>(AliveEnemies.Groups))
//            {
//                for (int i = 0; i < count; i++)
//                {
//                    enemies[i].animationComponent.playAnimation = "PlayerDead";
//                    enemies[i].movementComponent.navMeshEnabled = false;
//                }
//            }
//        }
//
//        public EntitiesDB entitiesDB { get; set; }
//        public void       Ready()    {  }
//    }
//}