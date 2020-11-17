using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class SortedDoofusesEnginesExecutionGroup : SortedJobifiedEnginesGroup<IJobifiedEngine, DoofusesEnginesOrder>
    {
        public SortedDoofusesEnginesExecutionGroup(FasterList<IJobifiedEngine> engines) : base(engines)
        {
        }
    }
}