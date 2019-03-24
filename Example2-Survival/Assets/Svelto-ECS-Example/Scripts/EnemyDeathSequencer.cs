using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive
{
    public class EnemyDeathSequencer : Sequencer<EnemyDeathSequencer>
    {
        public void SetSequence(EnemyDeathEngine     enemyDeathEngine, 
                                ScoreEngine          scoreEngine, 
                                DamageSoundEngine    damageSoundEngine, 
                                EnemyAnimationEngine enemyAnimationEngine, 
                                EnemySpawnerEngine   enemySpawnerEngine)
        {
            base.SetSequence(
                             new Steps //sequence of steps, this is a dictionary!
                                 (
                                  new Step
                                  {
                                      @from = enemyDeathEngine,
                                      to = new To
                                          (
                                           //TIP: use GO To Type Declaration to go directly to the Class code of the 
                                           //engine instance
                                           scoreEngine, damageSoundEngine
                                          )
                                  },
                                  new Step
                                  {
                                      //second step
                                      @from = enemyAnimationEngine, 
                                      //after the death animation is actually finished
                                      to = new To
                                          (
                                           enemySpawnerEngine //call the spawner engine
                                          )
                                  }
                                 )
                            );
        }
    }
}