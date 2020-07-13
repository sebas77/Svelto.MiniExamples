using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class EnginesExecutionOrderGroup : SortedJobifedEnginesGroup<IJobifiedEngine, DoofusesEnginesOrder>
    {
        public EnginesExecutionOrderGroup(FasterList<IJobifiedEngine> engines) : base(engines)
        {
        }
    }
}