using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IEngineScheduler
    {
        void ExecuteGraphics(FixedPoint delta, ulong ticks);
        void ExecutePhysics(FixedPoint delta, ulong @ulong);
        void RegisterScheduledGraphicsEngine(IScheduledGraphicsEngine scheduledGraphicsEngine);
        void RegisterScheduledPhysicsEngine(IScheduledPhysicsEngine scheduled);
    }
}