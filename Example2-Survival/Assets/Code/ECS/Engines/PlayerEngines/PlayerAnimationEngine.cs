using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStep<PlayerDeathCondition>
    {
        readonly ITaskRoutine<IEnumerator> _taskRoutine;

        public IEntitiesDB entitiesDB { get; set; }

        public void Ready() { PhysicsTick().RunOnScheduler(StandardSchedulers.physicScheduler); }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            uint index;
            var  playerEntityViews = entitiesDB.QueryEntitiesAndIndex<PlayerEntityViewStruct>(id, out index);

            playerEntityViews[index].animationComponent.playAnimation = "Die";
        }

        IEnumerator PhysicsTick()
        {
            while (true)
            {
                var playerEntities =
                    entitiesDB.QueryEntities<PlayerEntityViewStruct, PlayerInputDataStruct>(ECSGroups.Player,
                                                                                            out var count);

                for (var i = 0; i < count; i++)
                {
                    var input = playerEntities.Item2[i].input;

                    // Create a boolean that is true if either of the input axes is non-zero.
                    var walking = input.x != 0f || input.z != 0f;

                    // Tell the animator whether or not the player is walking.
                    playerEntities.Item1[i].animationComponent.animationState = new AnimationState("IsWalking", walking);
                }

                yield return null;
            }
        }
    }
}