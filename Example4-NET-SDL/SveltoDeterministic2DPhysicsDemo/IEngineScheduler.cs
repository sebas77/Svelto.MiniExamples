using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IEngineScheduler
    {
        void ExecuteGraphics(FixedPoint delta);
        void ExecutePhysics(FixedPoint delta);
        void RegisterScheduledGraphicsEngine(IScheduledGraphicsEngine scheduledGraphicsEngine);
        void RegisterScheduledPhysicsEngine(IScheduledPhysicsEngine scheduled);
    }
}