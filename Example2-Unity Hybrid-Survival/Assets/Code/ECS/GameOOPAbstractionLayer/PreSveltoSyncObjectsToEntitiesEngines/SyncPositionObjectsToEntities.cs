

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncPositionObjectsToEntities: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncPositionObjectsToEntities(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step()
        {
            var groups = entitiesDB
                   .FindGroups<GameObjectEntityComponent, PositionComponent>();

            //position only sync
            foreach (var ((entity, positions, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, PositionComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;

                    positions[i].position = transform.position;
                }
            }
            
            groups = entitiesDB
                   .FindGroups<GameObjectEntityComponent, RotationComponent>();

            //position only sync
            foreach (var ((entity, rotations, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, RotationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;

                    rotations[i].rotation = transform.rotation;
                }
            }
        }

        public string name => nameof(SyncPositionObjectsToEntities);
        readonly GameObjectResourceManager _manager;
    }
}