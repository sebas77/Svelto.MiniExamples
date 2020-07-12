using System.Collections;
using Svelto.Common;
using Svelto.ECS.Extensions;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    [Sequenced(nameof(EnginesEnum.PlayerAnimationEngine))]
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public PlayerAnimationEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _consumerFactory = consumerFactory;
            _deathCheck = DeathCheck();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        { }

        public void Step()
        {
            var groups =
                entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(ECSGroups.PlayersGroup);

            var (playersInput, playersView, count) = groups;

            for (var i = 0; i < count; i++)
            {
                var input = playersInput[i].input;

                // Create a boolean that is true if either of the input axes is non-zero.
                var walking = input.x != 0f || input.z != 0f;

                // Tell the animator whether or not the player is walking.
                playersView[i].animationComponent.animationState = new AnimationState("IsWalking", walking);
            }

            _deathCheck.MoveNext();
        }

        IEnumerator DeathCheck()
        {
            var consumer = _consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.PlayersGroup, "PlayerDeathEngine", 1);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out EGID id))
                {
                    var playerEntityView = entitiesDB.QueryEntity<PlayerEntityViewComponent>(id);

                    playerEntityView.animationComponent.playAnimation = "Die";
                }

                yield return null;
            }
        }

        public string name => nameof(PlayerAnimationEngine);
        
        readonly ITaskRoutine<IEnumerator>    _taskRoutine;
        readonly IEntityStreamConsumerFactory _consumerFactory;
        readonly IEnumerator _deathCheck;
    }
}