

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// Cameras set position directly, it should be considered an exception, as objects should be physic or path driven
    /// </summary>
    public class SyncEntitiesPositionToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncEntitiesPositionToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, PositionComponent>();
            //position only sync
            foreach (var ((entity, positions, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, PositionComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;

                    transform.position = positions[i].position;
                }
            }
        }

        public string name => nameof(SyncEntitiesPositionToObjects);

        readonly GameObjectResourceManager _manager;
    }
}