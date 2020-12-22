using Svelto.Common;
using Svelto.DataStructures;
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
    
    /// <summary>
    /// The order of the engines found in the enginesOrder array is the order of execution of engines
    /// </summary>
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
    
    public class SortedDoofusesEnginesExecutionGroup : SortedJobifiedEnginesGroup<IJobifiedEngine, DoofusesEnginesOrder>
    {
        public SortedDoofusesEnginesExecutionGroup(FasterList<IJobifiedEngine> engines) : base(engines)
        {
        }
    }
}