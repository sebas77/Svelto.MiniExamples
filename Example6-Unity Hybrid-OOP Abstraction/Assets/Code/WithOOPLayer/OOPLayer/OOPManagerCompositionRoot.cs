using Svelto.DataStructures;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// Encapsulated contexts (which provide engines, components and descriptors) can and should be packaged,
    /// for example in different assemblies, to promote encapsulation even further, through the use of the internal
    /// keyword
    /// </summary>
    public static class OOPManagerCompositionRoot
    {
        /// <summary>
        /// The Package composition root cannot hold any state and it's designed to be used by the outer
        /// composition root. Each encapsulated context has its own level of abstraction and a complex project
        /// could be made of different layers forming a tree of composition roots (Without cycles!). 
        /// </summary>
        public static void Compose
        (EnginesRoot enginesRoot, FasterList<IStepEngine> tickingEnginesGroup, uint maxQuantity)
        {
            var oopManager = new OOPManager();

            var syncEngine = new SyncTransformEngine(oopManager);
            var syncHierarchyEngine = new SyncHierarchyEngine(
                oopManager, enginesRoot.GenerateConsumerFactory(), maxQuantity);

            enginesRoot.AddEngine(syncEngine);
            enginesRoot.AddEngine(syncHierarchyEngine);
            enginesRoot.AddEngine(new SyncEntityCreation(oopManager));

            tickingEnginesGroup.Add(syncEngine);
            tickingEnginesGroup.Add(syncHierarchyEngine);
        }
    }
}