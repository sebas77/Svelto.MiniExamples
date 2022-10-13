using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive
{
    public enum DamageEnginesNames
    {
      DamageUnsortedEngines
    }
    
    [Sequenced(nameof(DamageEnginesNames.DamageUnsortedEngines))]
    public class DamageUnsortedEngines : UnsortedEnginesGroup<IStepEngine>
    {
        public DamageUnsortedEngines(FasterList<IStepEngine> engines) : base(engines)
        {
            
        }
    }
}