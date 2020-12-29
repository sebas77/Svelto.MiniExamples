using Svelto.DataStructures;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public static class OOPManagerCompositionRoot
    {
        public static void Compose
        (EnginesRoot enginesRoot, FasterList<IStepEngine> tickingEnginesGroup, out IOOPManager manager
       , uint maxQuantity)
        {
            var oopManager = new OOPManager();

            var syncEngine = new SyncTransformEngine(oopManager);
            var syncHierarchyEngine = new SyncHierarchyEngine(
                oopManager, enginesRoot.GenerateConsumerFactory(), maxQuantity);

            enginesRoot.AddEngine(syncEngine);
            enginesRoot.AddEngine(syncHierarchyEngine);

            tickingEnginesGroup.Add(syncEngine);
            tickingEnginesGroup.Add(syncHierarchyEngine);

            manager = oopManager;
        }
    }
}