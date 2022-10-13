using Svelto.Common;
using Svelto.ECS.Example.Survive.Player;
using Svelto.ECS.Example.Survive.Player.Gun;

namespace Svelto.ECS.Example.Survive
{
    public enum EnginesNames
    {
        SurvivalUnsortedEngines
      , UpdateScoreEngine
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
          , nameof(PlayerGunEnginesNames.PlayerGunShootingEngine)
          , nameof(EnemyEnginesNames.EnemyAttackEngine)
          , nameof(EnemyEnginesNames.EnemySpawnEffectOnDamage)
          , nameof(DamageEnginesNames.DamageUnsortedEngines)
          , nameof(EnemyEnginesNames.EnemyDeathEngine)
          , nameof(PlayerEnginesNames.PlayerDeathEngine)
          , nameof(EnginesNames.UpdateScoreEngine)
        };
    }
}