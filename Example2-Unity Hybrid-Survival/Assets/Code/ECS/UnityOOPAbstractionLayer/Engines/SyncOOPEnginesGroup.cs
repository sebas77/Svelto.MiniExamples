using Svelto.Common;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    [Sequenced(nameof(GameObjectsEnginesNames.SyncOOPEnginesGroup))]
    public class SyncOOPEnginesGroup: UnsortedEnginesGroup<IStepEngine>
    {
        
    }
}