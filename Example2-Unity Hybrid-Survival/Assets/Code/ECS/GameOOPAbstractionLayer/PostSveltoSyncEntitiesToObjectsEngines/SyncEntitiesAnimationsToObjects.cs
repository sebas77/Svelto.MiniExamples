using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncEntitiesAnimationsToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncEntitiesAnimationsToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, AnimationComponent>();
            //animation sync
            foreach (var ((entity, animations, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, AnimationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    ref var animationState = ref animations[i].animationState;

                    if (animationState.animationID != 0)
                    {
                        var go = _manager[entity[i].resourceIndex];

                        //could probably do with a check if the state actually changed
                        var animator = go.GetComponent<Animator>();

                        animator.SetBool(animationState.animationID, animationState.state);
                    }
                }
            }
        }

        public string name => nameof(SyncEntitiesAnimationsToObjects);

        readonly GameObjectResourceManager _manager;
    }
}