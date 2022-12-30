namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncEntitiesToGameObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncEntitiesToGameObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }
        
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent>();
            
            foreach (var ((gos, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent>(groups))
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    var go = _manager[gos[i].resourceIndex];

                    if (go.layer != gos[i].layer)
                        go.layer = gos[i].layer;
                }
            }
        }

        public string name => nameof(SyncEntitiesToGameObjects);
        
        readonly GameObjectResourceManager _manager;
    }
}