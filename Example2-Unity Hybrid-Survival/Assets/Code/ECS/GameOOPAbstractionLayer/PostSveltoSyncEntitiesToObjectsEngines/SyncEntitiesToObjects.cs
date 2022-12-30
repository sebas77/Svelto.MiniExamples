using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// Svelto engines have been executed, so entities have the latest values that must be copied to the gameobjects
    /// before the rendering of this frame is executed
    /// </summary>
    [Sequenced(nameof(GameObjectsEnginesNames.PostSveltoUpdateSyncEngines))]
    public class SyncEntitiesToObjects: UnsortedEnginesGroup<IStepEngine>
    { }
}