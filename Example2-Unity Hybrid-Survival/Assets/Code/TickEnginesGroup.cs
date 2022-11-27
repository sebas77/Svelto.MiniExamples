using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.Player.Gun;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// This struct is necessary to specify the order of execution of the engines by their type.
    /// As you can see from this example, this allow to fetch engines types from other assemblies
    /// </summary>
    public struct SortedTickedEnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
           nameof(PlayerGunEnginesNames.PlayerGunShootingEngine) 
//          , nameof(EnemyEnginesNames.EnemyAttackEngine)
//          , nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage)
//          , nameof(DamageEnginesNames.DamageUnsortedEngines)
//          , nameof(EnemyEnginesNames.EnemyDeathEngine)
          , nameof(PlayerEnginesNames.PlayerDeathEngine)
          , nameof(HUDEnginesNames.UpdateScoreEngine)
        };
    }

    /// <summary>
    /// Sorted engines, executed according to the order specified in SortedTickedEnginesOrder
    /// </summary>
    public class SortedEnginesGroup : SortedEnginesGroup<IStepEngine, SortedTickedEnginesOrder>
    {
        public SortedEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        {
        }
    }
    
    /// <summary>
    /// Unsorted engines, executed as found
    /// </summary>
    class SurvivalUnsortedEnginesGroup : UnsortedEnginesGroup<IStepEngine>
    {
        public SurvivalUnsortedEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        { }
    }
}