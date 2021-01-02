using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive
{
    [Sequenced(nameof(EnginesNames.SurvivalUnsortedEngines))]
    class SurvivalUnsortedEnginesGroup : UnsortedEnginesGroup<IStepEngine>
    {
        public SurvivalUnsortedEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        { }
    }
}