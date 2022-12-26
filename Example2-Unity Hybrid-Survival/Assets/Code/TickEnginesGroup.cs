using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.Player.Gun;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// This struct is necessary to specify the order of execution of the engines by their type.
    /// As you can see from this example, this allow to fetch engines types from other assemblies
    /// </summary>
    public struct SortedTickedEnginesOrder: ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
                nameof(GameObjectsEnginesNames.PreSveltoUpdateSyncEngines),
                nameof(TickEngineNames.UnsortedEngines),
                nameof(PlayerGunEnginesNames.PlayerGunShootingEngine),
                nameof(EnemyEnginesNames.EnemyAttackEngine),
                nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage), 
                nameof(DamageEnginesNames.DamageUnsortedEngines),
                nameof(EnemyEnginesNames.EnemyDeathEngine),
                nameof(PlayerEnginesNames.PlayerDeathEngine),
                nameof(HUDEnginesNames.UpdateScoreEngine),
                nameof(TickEngineNames.SubmissionEngine),
                nameof(GameObjectsEnginesNames.PostSveltoUpdateSyncEngines),
        };
    }

    public enum TickEngineNames
    {
        UnsortedEngines,
        SubmissionEngine
    }

    /// <summary>
    /// Sorted engines, executed according to the order specified in SortedTickedEnginesOrder
    /// </summary>
    public class SortedEnginesGroup: SortedEnginesGroup<IStepEngine, SortedTickedEnginesOrder>
    {
        public SortedEnginesGroup(FasterList<IStepEngine> engines): base(engines) { }
    }

    /// <summary>
    /// Unsorted engines, executed as found
    /// </summary>
    [Sequenced(nameof(TickEngineNames.UnsortedEngines))]
    class SurvivalUnsortedEnginesGroup: UnsortedEnginesGroup<IStepEngine>
    {
        public SurvivalUnsortedEnginesGroup(FasterList<IStepEngine> engines): base(engines) { }
    }
    
    [Sequenced(nameof(TickEngineNames.SubmissionEngine))]
    //When SyncEngines are used, it may be tricky to sync the values properly. The expectation for each frame is:
    //entities are sync with objects value
    //svelto engines run
    //objects are sync with entities value
    //unity stuff run
    //however for this to be correct, the complete sequence must be
    //entities are sync with objects value
    //svelto engines run
    //entities are submitted
    //objects are sync with entities value
    //unity stuff run
    class TickEngine: IStepEngine
    {
        public TickEngine(SimpleEntitiesSubmissionScheduler entitySubmissionScheduler)
        {
            _scheduler = entitySubmissionScheduler;
        }

        public void Step()
        {
            _scheduler.SubmitEntities();
        }

        public string name => nameof(TickEngine);
        
        readonly SimpleEntitiesSubmissionScheduler _scheduler;
    }
}