using Svelto.ECS.Example.Survive.Transformable;

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
                   .FindGroups<GameObjectEntityComponent, PositionComponent, RotationComponent>();

            //position only sync
            foreach (var ((entity, positions, rotations, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, PositionComponent, RotationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;

                    positions[i].position = transform.position;
                    rotations[i].rotation = transform.rotation;
                }
            }
        }

        public string name => nameof(SyncPositionObjectsToEntities);
        readonly GameObjectResourceManager _manager;
    }
}