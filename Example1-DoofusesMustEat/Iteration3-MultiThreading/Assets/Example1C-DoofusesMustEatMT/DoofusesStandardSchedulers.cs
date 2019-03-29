using Svelto.Tasks.ExtraLean.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public static class DoofusesStandardSchedulers
    {
        public static readonly UpdateMonoRunner foodScheduler = new UpdateMonoRunner("FoodLogic");
        public static readonly UpdateMonoRunner doofusesLogicScheduler = new UpdateMonoRunner("DoofusesLogic");
        public static readonly EarlyUpdateMonoRunner UIInteraction = new EarlyUpdateMonoRunner("UILogic");
        public static readonly LateMonoRunner rendererScheduler = new LateMonoRunner("Rendering");
        public static readonly EarlyUpdateMonoRunner physicScheduler = new EarlyUpdateMonoRunner("Physic");

        public static void StopAndCleanupAllDefaultSchedulers()
        {
            foodScheduler.Dispose();
            doofusesLogicScheduler.Dispose();
            UIInteraction.Dispose();
            rendererScheduler.Dispose();
            physicScheduler.Dispose();
        }
    }
}