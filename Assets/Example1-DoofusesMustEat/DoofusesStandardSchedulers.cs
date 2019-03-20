using Svelto.Tasks.ExtraLean.Unity;

namespace Svelto.ECS.MiniExamples.Example1
{
    public static class DoofusesStandardSchedulers
    {
        public static readonly UpdateMonoRunner foodScheduler = new UpdateMonoRunner("FoodScheduler");
    }
}