using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class PhysicsCore
    {
        public static void RegisterTo(EnginesRoot enginesRoot, IEngineScheduler scheduler)
        {
            var applyVelocityToRigidBodiesEngine = new ApplyVelocityToRigidBodiesEngine();
            var detectBoxVsBoxCollisionsEngine   = new DetectBoxVsBoxCollisionsEngine();
            var resolveCollisionEngine           = new ResolveCollisionEngine();
            var resolvePenetrationEngine         = new ResolvePenetrationEngine();
            var clearPerFrameStateEngine         = new ClearPerFrameStateEngine();

            enginesRoot.AddEngine(applyVelocityToRigidBodiesEngine);
            enginesRoot.AddEngine(detectBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(resolveCollisionEngine);
            enginesRoot.AddEngine(resolvePenetrationEngine);
            enginesRoot.AddEngine(clearPerFrameStateEngine);
            
            scheduler.RegisterScheduledPhysicsEngine(applyVelocityToRigidBodiesEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectBoxVsBoxCollisionsEngine);
            scheduler.RegisterScheduledPhysicsEngine(resolveCollisionEngine);
        }
    }
}