using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// The point of the OOPLayer is to encapsulate and abstract the use of objects. This is achievable through
    /// sync points. All the engines in this context are sync engines between entities and objects. Sync
    /// can be bi-directional.
    /// 
    /// </summary>
    public static class OOPLayerContext
    {
        public static void Setup(FasterList<IStepEngine> orderedEngines, EnginesRoot enginesRoot,
            GameObjectResourceManager gameObjectResourceManager)
        {
            var syncEntitiesToObjectsGroup = new SyncOOPEnginesGroup();

            IStepEngine syncEngine = null;

            syncEngine = new SyncCameraToObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncGameObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncGunToObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            var syncObjectsToEntities = new SyncPhysicToEntitiesEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncObjectsToEntities);
            
            orderedEngines.Add(syncEntitiesToObjectsGroup);
            orderedEngines.Add(syncObjectsToEntities);
        }
    }
}