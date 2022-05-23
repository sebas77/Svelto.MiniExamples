using Svelto.Tasks;

namespace Lean
{
    public static class BoxtopiaSchedulers
    {
        public static Svelto.Tasks.Lean.Unity.CoroutineMonoRunner UIScheduler =
            new Svelto.Tasks.Lean.Unity.CoroutineMonoRunner("Lean.Boxtopia.GUI");

        public static Svelto.Tasks.Lean.Unity.CoroutineMonoRunner UserScheduler =
            new Svelto.Tasks.Lean.Unity.CoroutineMonoRunner("Lean.User.GUI");

        public static Svelto.Tasks.Lean.Unity.CoroutineMonoRunner ResourceScheduler =
            new Svelto.Tasks.Lean.Unity.CoroutineMonoRunner("Lean.Boxtopia.Common.Resource");
    }
}

namespace ExtraLean
{
    public static class BoxtopiaSchedulers
    {
        public static Svelto.Tasks.ExtraLean.Unity.CoroutineMonoRunner UIScheduler =
            new Svelto.Tasks.ExtraLean.Unity.CoroutineMonoRunner("ExtraLean.Boxtopia.GUI");

        public static Svelto.Tasks.ExtraLean.Unity.EarlyUpdateMonoRunner InputScheduler =
            new Svelto.Tasks.ExtraLean.Unity.EarlyUpdateMonoRunner("ExtraLean.Boxtopia.Common.Input");

        public static Svelto.Tasks.ExtraLean.Unity.LateMonoRunner CharacterLateUpdateScheduler =
            new Svelto.Tasks.ExtraLean.Unity.LateMonoRunner("ExtraLean.Boxtopia.MainLevel.Character");

        public static Svelto.Tasks.ExtraLean.Unity.UpdateMonoRunner CharacterUpdateScheduler =
            new Svelto.Tasks.ExtraLean.Unity.UpdateMonoRunner("ExtraLean.Boxtopia.MainLevel.Character");
    }
}

public static class BoxtopiaSchedulers
{
    public static void StopAllCoroutines()
    {
        ExtraLean.BoxtopiaSchedulers.UIScheduler.Stop();
        ExtraLean.BoxtopiaSchedulers.InputScheduler.Stop();
        ExtraLean.BoxtopiaSchedulers.CharacterUpdateScheduler.Stop();
        ExtraLean.BoxtopiaSchedulers.CharacterLateUpdateScheduler.Stop();

        Lean.BoxtopiaSchedulers.UIScheduler.Stop();
        Lean.BoxtopiaSchedulers.UserScheduler.Stop();
        Lean.BoxtopiaSchedulers.ResourceScheduler.Stop();
    }

    public static void StopAndCleanupAllDefaultSchedulers()
    {
        ExtraLean.BoxtopiaSchedulers.UIScheduler.Dispose();
        ExtraLean.BoxtopiaSchedulers.InputScheduler.Dispose();
        ExtraLean.BoxtopiaSchedulers.CharacterUpdateScheduler.Dispose();
        ExtraLean.BoxtopiaSchedulers.CharacterLateUpdateScheduler.Dispose();

        Lean.BoxtopiaSchedulers.UIScheduler.Dispose();
        Lean.BoxtopiaSchedulers.UserScheduler.Dispose();
        Lean.BoxtopiaSchedulers.ResourceScheduler.Dispose();
    }
}

