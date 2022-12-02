using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public static class OOPLayerContext
    {
        public static void OOPLayerSetup(FasterList<IStepEngine> orderedEngines, EnginesRoot enginesRoot,
            GameObjectResourceManager gameObjectResourceManager)
        {
            var syncEngine = new SyncGameObjectsEngine(gameObjectResourceManager);
            enginesRoot.AddEngine(syncEngine);
            orderedEngines.Add(syncEngine);
        }
    }
}