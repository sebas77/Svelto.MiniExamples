using SveltoDeterministic2DPhysicsDemo.Maths;

namespace SveltoDeterministic2DPhysicsDemo
{
    public interface IEngineScheduler
    {
        void ExecuteGraphics(FixedPoint delta, ulong tick);
        void ExecutePhysics(ulong tick);
        void RegisterScheduledGraphicsEngine(IScheduledGraphicsEngine scheduledGraphicsEngine);
        void RegisterScheduledPhysicsEngine(IScheduledPhysicsEngine scheduled);
    }
}