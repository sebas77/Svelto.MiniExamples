using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public static class OOPLayerContext
    {
        public static void Setup(FasterList<IStepEngine> orderedEngines, EnginesRoot enginesRoot,
            GameObjectResourceManager gameObjectResourceManager)
        {
            var group = new SyncOOPEnginesGroup();

            IStepEngine syncEngine = null;

            syncEngine = new SyncCameraToObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            group.Add(syncEngine);
            
            syncEngine = new SyncGameObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            group.Add(syncEngine);
            
            syncEngine = new SyncGunToObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            group.Add(syncEngine);
            
            orderedEngines.Add(group);
        }
    }
}