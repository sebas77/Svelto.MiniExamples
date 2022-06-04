using Svelto.ECS.MiniExamples.Doofuses.GameObjects.GameobjectLayer;
using UnityEngine;

namespace Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// </summary>
    class InstantiateGameObjectOnSveltoEntityEngine : IQueryingEntitiesEngine
                                                    , IReactOnAddAndRemoveEx<GameObjectEntityComponent>
                                                    , IReactOnSwapEx<GameObjectEntityComponent>
    {
        public InstantiateGameObjectOnSveltoEntityEngine(ECSGameObjectsEntityManager goManager)
        {
            _goManager = goManager;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public void Add
        ((uint start, uint end) rangeOfEntities, in EntityCollection<GameObjectEntityComponent> collection
       , ExclusiveGroupStruct groupID)
        {
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
            {
                var (buffer, _) = collection;

                ref var entityComponent = ref buffer[i];

                var gameObjectID = _goManager.InstantiatePrefab(entityComponent.prefabID, groupID.id);

                gameObjectID.SetPositionAndRotation(entityComponent.spawnPosition.xyz, Quaternion.identity);
            }
        }

        public void Remove
        ((uint start, uint end) rangeOfEntities, in EntityCollection<GameObjectEntityComponent> collection
       , ExclusiveGroupStruct groupID)
        {
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
            {
                var (buffer, _, _) = collection;

                ref var entityComponent = ref buffer[i];

                _goManager.Recycle(entityComponent.prefabID, groupID.id);
            }
        }

        public void MovedTo
        ((uint start, uint end) rangeOfEntities, in EntityCollection<GameObjectEntityComponent> collection
       , ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
            {
                var (buffer, _, _) = collection;
                
                ref var entityComponent = ref buffer[i];

                _goManager.Swap(fromGroup.id, toGroup.id);
            }
        }

        readonly ECSGameObjectsEntityManager _goManager;
    }
}