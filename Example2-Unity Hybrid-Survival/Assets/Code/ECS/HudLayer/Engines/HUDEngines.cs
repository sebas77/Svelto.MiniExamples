using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.HUD
{
    [Sequenced(nameof(HUDEnginesNames.HUDEngines))]
    public class HUDEngines: UnsortedEnginesGroup<IStepEngine>
    {
        public HUDEngines(FasterList<IStepEngine> engines): base(engines) {}
    }
}