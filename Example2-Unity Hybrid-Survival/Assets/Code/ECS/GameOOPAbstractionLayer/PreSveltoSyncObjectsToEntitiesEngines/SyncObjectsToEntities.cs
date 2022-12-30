using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    [Sequenced(nameof(GameObjectsEnginesNames.PreSveltoUpdateSyncEngines))]
    public class SyncObjectsToEntities: UnsortedEnginesGroup<IStepEngine>
    { }
}