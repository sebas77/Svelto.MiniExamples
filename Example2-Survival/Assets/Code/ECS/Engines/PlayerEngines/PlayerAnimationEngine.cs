using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerAnimationEngine
        : SingleEntityReactiveEngine<PlayerEntityViewStruct>, IQueryingEntitiesEngine, IStep<PlayerDeathCondition>
    {
        readonly ITaskRoutine<IEnumerator> _taskRoutine;

        public PlayerAnimationEngine()
        {
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine(StandardSchedulers.physicScheduler);
            _taskRoutine.SetEnumerator(PhysicsTick());
        }

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { _taskRoutine.Start(); }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            uint index;
            var  playerEntityViews = entitiesDB.QueryEntitiesAndIndex<PlayerEntityViewStruct>(id, out index);

            playerEntityViews[index].animationComponent.playAnimation = "Die";
        }

        IEnumerator PhysicsTick()
        {
            //wait for the player to spawn
            while (entitiesDB.HasAny<PlayerEntityViewStruct>(ECSGroups.Player) == false)
                yield return null; //skip a frame

            var playerEntityViews =
                entitiesDB.QueryEntities<PlayerEntityViewStruct>(ECSGroups.Player, out var targetsCount);
            var playerInputDatas = entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out targetsCount);

            while (true)
            {
                var input = playerInputDatas[0].input;

                // Create a boolean that is true if either of the input axes is non-zero.
                var walking = input.x != 0f || input.z != 0f;

                // Tell the animator whether or not the player is walking.
                playerEntityViews[0].animationComponent.animationState = new AnimationState("IsWalking", walking);

                yield return null;
            }
        }

        protected override void Add(ref PlayerEntityViewStruct           entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
        }

        protected override void Remove(ref PlayerEntityViewStruct entityView, bool itsaSwap) { _taskRoutine.Stop(); }
    }
}