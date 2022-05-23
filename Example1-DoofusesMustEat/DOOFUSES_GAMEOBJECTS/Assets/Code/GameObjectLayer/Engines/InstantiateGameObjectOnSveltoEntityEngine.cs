namespace Svelto.ECS.MiniExamples.GameObjectsLayer
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
        public InstantiateGameObjectOnSveltoEntityEngine(GameObjectManager goManager)
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

                var gameObjectID = _goManager.FetchGameObject(entityComponent.prefabID, (int)(uint)groupID.id);

                _goManager.SetPosition(gameObjectID, (int)(uint)groupID.id, entityComponent.spawnPosition);

                entityComponent.gameObjectID = gameObjectID;
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

                _goManager.Recycle(entityComponent.gameObjectID, (int)(uint)groupID.id);
            }
        }

        public void MovedTo
        ((uint start, uint end) rangeOfEntities, in EntityCollection<GameObjectEntityComponent> collection
       , ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            Remove(rangeOfEntities, collection, fromGroup);
            Add(rangeOfEntities, collection, toGroup);
        }

        readonly GameObjectManager _goManager;
    }
}