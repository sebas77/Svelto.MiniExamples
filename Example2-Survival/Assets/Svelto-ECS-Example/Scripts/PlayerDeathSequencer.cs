using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.Characters.Player;
using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive
{
    public class PlayerDeathSequencer : Sequencer<PlayerDeathSequencer>
    {
        public void SetSequence(PlayerDeathEngine                     playerDeathEngine,
                                PlayerMovementEngine playerMovementEngine,
                                PlayerAnimationEngine playerAnimationEngine,
                                EnemyAnimationEngine enemyAnimationEngine,
                                DamageSoundEngine damageSoundEngine,
                                HUDEngine hudEngine)
        {
            base.SetSequence(
                             new Steps //sequence of steps, this is a dictionary!
                                 (
                                  new Step
                                  {
                                      from = playerDeathEngine, //when the player dies
                                      to = new To<PlayerDeathCondition>
                                          //all these engines in the list will be called in order (which in this 
                                          //case was not important at all, so stretched!!)
                                          {
                                              {
                                                  PlayerDeathCondition.Death, playerMovementEngine,
                                                  playerAnimationEngine,
                                                  enemyAnimationEngine, damageSoundEngine, hudEngine
                                              }
                                          }
                                  }
                                 ));

        }
    }
}