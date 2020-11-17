using Svelto.Common;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public enum DoofusesEngineNames
    {
        SpawningDoofusEngine
      , PlaceFoodOnClickEngine
      , LookingForFoodDoofusesEngine
      , VelocityToPositionDoofusesEngine
      , ConsumingFoodEngine
    }

    public struct DoofusesEnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
            nameof(DoofusesEngineNames.SpawningDoofusEngine)
          , nameof(DoofusesEngineNames.PlaceFoodOnClickEngine)
          , nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine)
          , nameof(DoofusesEngineNames.ConsumingFoodEngine)
          , nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine)
          , nameof(JobifiedSveltoEngines.SveltoOverUECS)
        };
    }
}