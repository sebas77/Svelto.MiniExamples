using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class PhysicsCore
    {
        public static void RegisterTo
            (EnginesRoot enginesRoot, IEngineScheduler scheduler)
        {
            enginesRoot.AddEngine(new ApplyVelocityEngine(scheduler));
            enginesRoot.AddEngine(new DetectBoxVsBoxCollisionsEngine(scheduler));
            enginesRoot.AddEngine(new ResolveCollisionEngine(scheduler));
            enginesRoot.AddEngine(new ResolvePenetrationEngine(scheduler));
            enginesRoot.AddEngine(new ClearPerFrameStateEngine(scheduler));
        }
    }
}