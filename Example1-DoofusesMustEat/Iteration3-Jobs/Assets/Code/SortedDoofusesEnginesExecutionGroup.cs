using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    ///it's important to note that the names of the engines used in the ISequenceOrder, do NOT need to come from the
    /// same enum. This will allow the user to declare enums in their own assemblies. 
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
    //The engines added in this group will need to be marked as Sequenced and signed with the enums used inside
    //the ISequenceOrder struct. If the ISequenceOrder names and the signatures do not match, Svelto will throw an
    //exception. If they match, the engines will be executed using the ISequencedOrder declaration.
    public class SortedDoofusesEnginesExecutionGroup : SortedJobifiedEnginesGroup<IJobifiedEngine, DoofusesEnginesOrder>
    {
        public SortedDoofusesEnginesExecutionGroup(FasterList<IJobifiedEngine> engines) : base(engines)
        {
        }
    }
}