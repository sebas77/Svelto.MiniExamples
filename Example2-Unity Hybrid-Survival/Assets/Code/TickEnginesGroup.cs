using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.Player.Gun;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// This struct is necessary to specify the order of execution of the engines by their type.
    /// As you can see from this example, this allow to fetch engines types from other assemblies
    /// When SyncEngines are used, it may be tricky to sync the values properly. The expectation for each frame is:
    /// entities are sync with objects value
    /// svelto engines run
    /// objects are sync with entities value
    /// unity stuff run
    /// however for this to be correct, the complete sequence must be
    /// entities are sync with objects value
    /// svelto engines run
    /// entities are submitted
    /// objects are sync with entities value
    /// unity stuff run
    /// </summary>
    public struct SortedTickedEnginesOrder: ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
                nameof(GameObjectsEnginesNames.PreSveltoUpdateSyncEngines),
                nameof(TickEngineNames.UnsortedEngines),
                nameof(PlayerGunEnginesNames.PlayerFiresGunEngine),
                nameof(EnemyEnginesNames.EnemyAttackEngine),
                nameof(DamageEnginesNames.DamageUnsortedEngines),
                nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage), 
                nameof(EnemyEnginesNames.EnemyDeathEngine),
                nameof(PlayerEnginesNames.PlayerDamagedEngine),
                nameof(PlayerEnginesNames.PlayerDeathEngine),
                nameof(HUDEnginesNames.HUDEngines),
                //SubmissionEngine awkward note: enemies spawned on this frame won't synchronise to the gameobjects
                //if they are not submitted before PostSveltoUpdate is called. A solution to the problem could be to
                //react on add so the synchronization happens during the submission, but this would mean to have two
                //sync points: one every frame and one on creation
                //on the other hand, entities removed this frame won't be synchronised, so if sounds play on death
                //they won't be played because the entities are already removed. Again this could be solved with
                //a react on remove.
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