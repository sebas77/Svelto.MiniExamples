using Svelto.Common;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public enum DoofusesEngineNames
    {
        PlaceFoodOnClickEngine,
        LookingForFoodDoofusesEngine,
        SpawningDoofusEngine,
        VelocityToPositionDoofusesEngine,
        ConsumingFoodEngine
    }
    
    public struct DoofusesEnginesOrder : ISequenceOrder
    {
        public string[] enginesOrder => new[]
        {
            nameof(DoofusesEngineNames.SpawningDoofusEngine),
            nameof(DoofusesEngineNames.PlaceFoodOnClickEngine),
            nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine),
            nameof(DoofusesEngineNames.ConsumingFoodEngine),
            nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine),
            nameof(JobifiedSveltoEngines.CopySveltoToUECSEnginesGroup), //Tick our dependency this is the special group
            nameof(JobifiedSveltoEngines.PureUECSSystemsGroup), //
        };
    }
}