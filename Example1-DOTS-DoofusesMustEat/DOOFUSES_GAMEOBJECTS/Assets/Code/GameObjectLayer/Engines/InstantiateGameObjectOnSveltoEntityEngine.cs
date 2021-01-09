namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// </summary>
    class InstantiateGameObjectOnSveltoEntityEngine : IQueryingEntitiesEngine
                                                    , IReactOnAddAndRemove<GameObjectEntityComponent>, IReactOnSwap<GameObjectEntityComponent>
    {
        public InstantiateGameObjectOnSveltoEntityEngine(GameObjectManager goManager) { _goManager = goManager; }
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public void Add(ref GameObjectEntityComponent entityComponent, EGID egid)
        {
            var gameObjectID = _goManager.FetchGameObject(entityComponent.prefabID, (int)(uint)egid.groupID);

            _goManager.SetPosition(gameObjectID, (int)(uint)egid.groupID, entityComponent.spawnPosition);

            entityComponent.gameObjectID = gameObjectID;
        }

        public void Remove(ref GameObjectEntityComponent entityComponent, EGID egid)
        {
            _goManager.Recycle(entityComponent.gameObjectID, (int)(uint)egid.groupID);
        }

        public void MovedTo
            (ref GameObjectEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            Remove(ref entityComponent, new EGID(egid.entityID, previousGroup));
            Add(ref entityComponent, egid);
        }
        
        readonly GameObjectManager _goManager;
    }
}