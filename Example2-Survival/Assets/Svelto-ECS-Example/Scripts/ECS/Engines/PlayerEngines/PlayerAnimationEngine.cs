using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerAnimationEngine : SingleEntityEngine<PlayerEntityViewStruct>, IQueryingEntitiesEngine, IStep<PlayerDeathCondition>
    {
        public IEntitiesDB entitiesDB { get; set; }
        public void Ready()
        {
            _taskRoutine.Start();
        }
        
        public PlayerAnimationEngine()
        {
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine(StandardSchedulers.physicScheduler);
                _taskRoutine.SetEnumerator(PhysicsTick());
        }
        
        IEnumerator PhysicsTick()
        {
            //wait for the player to spawn
            while (entitiesDB.HasAny<PlayerEntityViewStruct>(ECSGroups.Player) == false)
            {
                yield return null; //skip a frame
            }

            int targetsCount;
            var playerEntityViews = entitiesDB.QueryEntities<PlayerEntityViewStruct>(ECSGroups.Player, out targetsCount);
            var playerInputDatas = entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out targetsCount);
            
            while (true)
            {
                var input = playerInputDatas[0].input;

                // Create a boolean that is true if either of the input axes is non-zero.
                bool walking = input.x != 0f || input.z != 0f;

                // Tell the animator whether or not the player is walking.
                playerEntityViews[0].animationComponent.animationState = new AnimationState("IsWalking", walking);

                yield return null;
            }
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            uint index;
            var  playerEntityViews = entitiesDB.QueryEntitiesAndIndex<PlayerEntityViewStruct>(id, out index);
            
            playerEntityViews[index].animationComponent.playAnimation = "Die";
        }

        protected override void Add(ref PlayerEntityViewStruct entityView)
        {}

        protected override void Remove(ref PlayerEntityViewStruct entityView)
        {
            _taskRoutine.Stop();
        }
        
        readonly ITaskRoutine<IEnumerator> _taskRoutine;
    }
}
