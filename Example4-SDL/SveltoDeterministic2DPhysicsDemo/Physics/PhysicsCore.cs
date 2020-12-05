using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics.Engines;

namespace SveltoDeterministic2DPhysicsDemo.Physics
{
    public static class PhysicsCore
    {
        public static void RegisterTo
            (EnginesRoot enginesRoot, IEngineScheduler scheduler, FixedPoint physicsSimulationsPerSecond)
        {
            enginesRoot.AddEngine(new ApplyVelocityEngine(scheduler, physicsSimulationsPerSecond));
            enginesRoot.AddEngine(new DetectBoxVsBoxCollisionsEngine(scheduler));
            enginesRoot.AddEngine(new ResolveCollisionEngine(scheduler));
            enginesRoot.AddEngine(new ResolvePenetrationEngine(scheduler));
            enginesRoot.AddEngine(new ClearPerFrameStateEngine(scheduler));
        }
    }
}