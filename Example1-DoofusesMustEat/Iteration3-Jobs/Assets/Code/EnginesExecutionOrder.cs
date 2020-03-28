using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class EnginesExecutionOrder : JobifiableEnginesGroup<IJobifiableEngine, DoofusesEnginesOrder>
    {
        public EnginesExecutionOrder(FasterReadOnlyList<IJobifiableEngine> engines) : base(engines)
        {
        }
    }
}