using Svelto.Common;
using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive
{
    public enum EnginesNames
    {
        SurvivalUnsortedEngines
      , UpdateScoreEngine
       , DamageUnsortedEngines
    }

    public enum EnemyEnginesNames
    {
        EnemyAttackEngine
      , EnemyDeathEngine
      , EnemySpawnEffectOnDamage
    }

    public struct SortedTickedEnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
            nameof(EnginesNames.SurvivalUnsortedEngines)
          , nameof(PlayerEnginesNames.PlayerGunShootingEngine)
          , nameof(EnemyEnginesNames.EnemyAttackEngine)
          , nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage)
          , nameof(EnginesNames.DamageUnsortedEngines)
          , nameof(EnemyEnginesNames.EnemyDeathEngine)
          , nameof(PlayerEnginesNames.PlayerDeathEngine)
          , nameof(EnginesNames.UpdateScoreEngine)
        };
    }
}