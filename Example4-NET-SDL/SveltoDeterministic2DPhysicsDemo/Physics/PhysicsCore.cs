using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    /// <summary>
    /// Physic core shows an interesting concept that is used in Gamecraft too: packaging encapsulated contexts
    /// in different assemblies and exposing only the method to register engines.
    /// </summary>
    public static class PhysicsCore
    {
        public static void RegisterEngines(in EnginesRoot enginesRoot, in IEngineScheduler scheduler)
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