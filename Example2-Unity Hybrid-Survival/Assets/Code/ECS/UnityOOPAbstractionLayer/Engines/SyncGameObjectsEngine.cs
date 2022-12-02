using Svelto.Common;
using Svelto.ECS.Example.Survive.Transformable;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    [Sequenced(nameof(GameObjectsEnginesNames.SyncGameObjectsEngine))]
    public class SyncGameObjectsEngine:  IQueryingEntitiesEngine, IStepEngine
    {
        public SyncGameObjectsEngine(GameObjectResourceManager manager) 
        {
            _manager = manager;
        }

        public void Ready()
        { }

        public EntitiesDB entitiesDB { get; set; }
        
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, RotationComponent, RigidBodyComponent, PositionComponent>();
            
            foreach (var ((entity, rotation, rbs, positions, count), _) in entitiesDB
                        .QueryEntities<GameObjectEntityComponent, RotationComponent, RigidBodyComponent,PositionComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var rb = go.GetComponent<Rigidbody>(); //in a real project I'd cached this
                    
                    rb.velocity = rbs[i].velocity;
                    rb.isKinematic = rbs[i].isKinematic;
                }
                
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var transform = go.transform;

                    positions[i].position = transform.position;
                    transform.rotation = rotation[i].rotation;
                }
            }
        }

        public string name => nameof(SyncGameObjectsEngine);
        readonly GameObjectResourceManager _manager;
    }
}