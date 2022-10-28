using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Damage
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