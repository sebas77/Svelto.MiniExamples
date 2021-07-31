using Svelto.Common;

namespace Svelto.ECS.Example.Survive
{
    public enum EnginesNames
    {
        SurvivalUnsortedEngines
      , UpdateScoreEngine
      , UpdateWaveHUDEngine
      , UpdateAmmoEngine 
      , DamageUnsortedEngines
    }

    public enum PlayerEnginesNames
    {
       PlayerDeathEngine
      , PlayerGunShootingEngine
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
          , nameof(EnginesNames.UpdateWaveHUDEngine)
          , nameof(EnginesNames.UpdateAmmoEngine)
        };
    }
}