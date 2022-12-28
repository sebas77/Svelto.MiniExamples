using System;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncCollisionsToEntities: IQueryingEntitiesEngine, IReactOnAddEx<CollisionComponent>
    {
        public SyncCollisionsToEntities(GameObjectResourceManager manager)
        {
            _manager = manager;
            _onCollidedWithTarget = OnCollidedWithTarget;
        }

        public void Ready() { }
        public EntitiesDB entitiesDB { get; set; }
        
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<CollisionComponent> entities, ExclusiveGroupStruct groupID)
        {
            var (gos, _) = entitiesDB.QueryEntities<GameObjectEntityComponent>(groupID);

            for (int i = (int)(rangeOfEntities.end - 1); i >= rangeOfEntities.start; i--)
            {
                _manager[gos[i].resourceIndex].GetComponent<ObjectTrigger>().Register(_onCollidedWithTarget);
            }
        }
        
        /// <summary>
        /// once an object enters in a trigger, we set the trigger data built inside the implementor and sent
        /// through the DispatchOnChange
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="enemyCollisionData"></param>
        void OnCollidedWithTarget(EntityReference sender, CollisionData collisionData)
        {
            entitiesDB.QueryEntity<CollisionComponent>(sender.ToEGID(entitiesDB)).entityInRange = collisionData;
        }

        public string name => nameof(SyncCollisionsToEntities);

        readonly GameObjectResourceManager _manager;
        readonly Action<EntityReference, CollisionData> _onCollidedWithTarget;
    }
}