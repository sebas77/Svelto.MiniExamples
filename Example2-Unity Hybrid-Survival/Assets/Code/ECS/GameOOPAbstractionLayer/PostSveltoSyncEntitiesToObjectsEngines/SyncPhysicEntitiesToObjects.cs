
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncPhysicEntitiesToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncPhysicEntitiesToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            //Find all the subsets of entities that contains all the following components
            var groups = entitiesDB
                   .FindGroups<GameObjectEntityComponent, RotationComponent, RigidBodyComponent, PositionComponent>();

            //Iterating these entities and sync the values
            foreach (var ((entity, rotation, rbs, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, RotationComponent, RigidBodyComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;
                    var rb = go.GetComponent<Rigidbody>(); //in a real project I'd cached this

                    rb.velocity = rbs[i].velocity;
                    rb.isKinematic = rbs[i].isKinematic;
                    
                    transform.rotation = rotation[i].rotation;
                }
            }
        }

        public string name => nameof(SyncPhysicEntitiesToObjects);

        readonly GameObjectResourceManager _manager;
    }
}