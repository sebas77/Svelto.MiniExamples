using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive
{
    [Sequenced(nameof(EnginesNames.DamageUnsortedEngines))]
    class DamageUnsortedEngines : UnsortedEnginesGroup<IStepEngine>
    {
        public DamageUnsortedEngines(FasterList<IStepEngine> engines) : base(engines)
        {
            
        }
    }
}