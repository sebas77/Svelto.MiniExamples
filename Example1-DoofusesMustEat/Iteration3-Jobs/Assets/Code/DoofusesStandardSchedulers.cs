using Svelto.Tasks.ExtraLean.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public static class DoofusesStandardSchedulers
    {
        public static readonly UpdateMonoRunner mainThreadScheduler = new UpdateMonoRunner("MainThreadScheduler");

        public static void StopAndCleanupAllDefaultSchedulers()
        {
            mainThreadScheduler.Dispose();
        }
    }
}