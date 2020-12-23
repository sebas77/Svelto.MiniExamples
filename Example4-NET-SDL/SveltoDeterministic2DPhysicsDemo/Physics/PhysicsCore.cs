using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class PhysicsCore
    {
        public static void RegisterTo(in EnginesRoot enginesRoot, in IEngineScheduler scheduler)
        {
            var applyVelocityToRigidBodiesEngine                 = new ApplyVelocityToRigidBodiesEngine();
            var detectDynamicBoxVsBoxCollisionsEngine            = new DynamicBoxVsBoxCollisionsEngine();
            var detectDynamicVsKinematicBoxVsBoxCollisionsEngine = new DynamicVsKinematicBoxVsBoxCollisionsEngine();

            enginesRoot.AddEngine(applyVelocityToRigidBodiesEngine);
            enginesRoot.AddEngine(detectDynamicBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);

            scheduler.RegisterScheduledPhysicsEngine(applyVelocityToRigidBodiesEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectDynamicBoxVsBoxCollisionsEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);
        }
    }
}