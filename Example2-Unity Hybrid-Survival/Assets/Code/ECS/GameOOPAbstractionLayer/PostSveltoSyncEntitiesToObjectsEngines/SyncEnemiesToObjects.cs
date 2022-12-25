namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncEnemiesToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncEnemiesToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }
        
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            //only enemies
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, EnemyOOPComponent>();
            
            foreach (var ((gos, enemies, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, EnemyOOPComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[gos[i].resourceIndex];

                    go.layer = enemies[i].layer;
                }
            }
        }

        public string name => nameof(SyncEnemiesToObjects);
        
        readonly GameObjectResourceManager _manager;
    }
}