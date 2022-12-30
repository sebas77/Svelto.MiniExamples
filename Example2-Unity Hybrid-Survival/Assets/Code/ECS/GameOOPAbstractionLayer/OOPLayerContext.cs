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
            var syncEntitiesToObjectsGroup = new SyncEntitiesToObjects();
            var syncObjectsToEntitiesGroup = new SyncObjectsToEntities();

            IStepEngine syncEngine = null;
            
            //pre-svelto engines
            syncEngine = new SyncObjectsToGuns(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncObjectsToEntitiesGroup.Add(syncEngine);
            
            syncEngine = new SyncCameraObjectsToEntities(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncObjectsToEntitiesGroup.Add(syncEngine);
            
            syncEngine = new SyncPositionObjectsToEntities(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncObjectsToEntitiesGroup.Add(syncEngine);
            
            enginesRoot.AddEngine(new SyncCollisionsToEntities(gameObjectResourceManager)); //does not step
            
            orderedEngines.Add(syncObjectsToEntitiesGroup);
            
            
            //post-svelto engines
            syncEngine = new SyncEntitiesAnimationsToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncEntitiesPositionToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncPhysicEntitiesToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncGunEntitiesToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncEntitiesToGameObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncEntitiesAudioToObject(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncVFXEntitiesToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            syncEngine = new SyncNavToObjects(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            syncEntitiesToObjectsGroup.Add(syncEngine);
            
            orderedEngines.Add(syncEntitiesToObjectsGroup);
        }
    }
}