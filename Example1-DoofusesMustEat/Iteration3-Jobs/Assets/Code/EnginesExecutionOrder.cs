using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class EnginesExecutionOrder : SortedJobifedEnginesGroup<IJobifiedEngine, DoofusesEnginesOrder>
    {
        public EnginesExecutionOrder(FasterReadOnlyList<IJobifiedEngine> engines) : base(engines)
        {
        }
    }
}