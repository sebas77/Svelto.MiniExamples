using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    [Sequenced(nameof(GameObjectsEnginesNames.PostSveltoUpdateSyncEngines))]
    public class SyncOOPEnginesGroup: UnsortedEnginesGroup<IStepEngine>
    { }
}