using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.Transformable;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncGameObjectsEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncGameObjectsEngine(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step()
        {
            var groups = entitiesDB
               .FindGroups<GameObjectEntityComponent, RotationComponent, RigidBodyComponent, PositionComponent>();

            //RB sync
            foreach (var ((entity, rotation, rbs, positions, count), _) in entitiesDB
                        .QueryEntities<GameObjectEntityComponent, RotationComponent, RigidBodyComponent,
                             PositionComponent>(groups))
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

            groups = entitiesDB.FindGroups<GameObjectEntityComponent, AnimationComponent>();
            //animation sync
            foreach (var ((entity, animations, count), _) in entitiesDB
                        .QueryEntities<GameObjectEntityComponent, AnimationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var go = _manager[entity[i].resourceIndex];

                    var animator =
                        go.GetComponent<Animator>(); //could probably do with a check if the state actually changed

                    ref var animationState = ref animations[i].animationState;
                    animator.SetBool(animationState.animationID, animationState.state);
                }
            }

            groups = entitiesDB.FindGroups<GameObjectEntityComponent, PositionComponent>();
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

        public string name => nameof(SyncGameObjectsEngine);
        readonly GameObjectResourceManager _manager;
    }
}