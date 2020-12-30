using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive
{
    public class DamageEngineGroup<T> : SortedEnginesGroup<IStepEngine, T> where T : struct, ISequenceOrder
    {
        public DamageEngineGroup(FasterList<IStepEngine> engines) : base(engines)
        {
            Loop().Run();
        }

        IEnumerator Loop()
        {
            while (true)
            {
                this.StepAll();
                
                yield return null;
            }
        }
    }

    public enum EnginesEnum
    {
        EnemyAttackEngine
      , ApplyDamageEngine
      , DeathEngine
      , PlayerAnimationEngine
      , PlayerDeathEngine
       , PlayerGunShootingEngine
        , EnemyDeathEngine
    }
    
    public struct EnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
            nameof(EnginesEnum.PlayerGunShootingEngine),
            nameof(EnginesEnum.EnemyAttackEngine)
          , nameof(EnginesEnum.ApplyDamageEngine)
          , nameof(EnginesEnum.DeathEngine)
          , nameof(EnginesEnum.PlayerAnimationEngine)
          , nameof(EnginesEnum.PlayerDeathEngine)
          , nameof(EnginesEnum.EnemyDeathEngine)
        };
    }
}