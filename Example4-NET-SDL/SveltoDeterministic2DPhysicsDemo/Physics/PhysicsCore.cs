using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class PhysicsCore
    {
        public static void RegisterTo(EnginesRoot enginesRoot, IEngineScheduler scheduler)
        {
            var applyVelocityToRigidBodiesEngine                 = new ApplyVelocityToRigidBodiesEngine();
            var detectDynamicBoxVsBoxCollisionsEngine            = new DetectDynamicBoxVsBoxCollisionsEngine();
            var detectDynamicVsKinematicBoxVsBoxCollisionsEngine = new DetectDynamicVsKinematicBoxVsBoxCollisionsEngine();
            var resolveCollisionEngine                           = new ResolveCollisionEngine();
            var resolvePenetrationEngine                         = new ResolvePenetrationEngine();
            var applyImpulseEngine                               = new ApplyImpulseEngine();
            var clearPerFrameStateEngine                         = new ClearPerFrameStateEngine();

            enginesRoot.AddEngine(applyVelocityToRigidBodiesEngine);
            enginesRoot.AddEngine(detectDynamicBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(resolveCollisionEngine);
            enginesRoot.AddEngine(resolvePenetrationEngine);
            enginesRoot.AddEngine(applyImpulseEngine);
            enginesRoot.AddEngine(clearPerFrameStateEngine);

            scheduler.RegisterScheduledPhysicsEngine(applyVelocityToRigidBodiesEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectDynamicBoxVsBoxCollisionsEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);
            scheduler.RegisterScheduledPhysicsEngine(resolveCollisionEngine);
            scheduler.RegisterScheduledPhysicsEngine(resolvePenetrationEngine);
            scheduler.RegisterScheduledPhysicsEngine(applyImpulseEngine);
            scheduler.RegisterScheduledPhysicsEngine(clearPerFrameStateEngine);
        }
    }
}