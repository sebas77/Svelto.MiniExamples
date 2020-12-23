using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DynamicVsKinematicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(in FixedPoint delta)
        {
            var kinematicEntities = entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(GameGroups.KinematicRigidBodyWithBoxColliders.Groups);
            var dynamicEntities   = entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(GameGroups.DynamicRigidBodyWithBoxColliders.Groups);

            // Detect dynamic vs kinematic collisions.
            foreach (var ((kinematicTransforms, kinematicRigidBodies, kinematicColliders, kinematicCount), _) in kinematicEntities)
            {
                for (var kinematicIndex = 0; kinematicIndex < kinematicCount; kinematicIndex++)
                {
                    ref var transformKinematic = ref kinematicTransforms[kinematicIndex];
                    ref var rigidBodyKinematic = ref kinematicRigidBodies[kinematicIndex];
                    ref var colliderKinematic  = ref kinematicColliders[kinematicIndex];

                    var aabbKinematic = colliderKinematic.ToAABB(transformKinematic.Position);

                    foreach (var ((dynamicTransforms, dynamicRigidBodies, dynamicColliders, dynamicCount), _) in dynamicEntities)
                    {
                        for (var dynamicIndex = 0; dynamicIndex < dynamicCount; dynamicIndex++)
                        {
                            ref var transformDynamic = ref dynamicTransforms[dynamicIndex];
                            ref var rigidBodyDynamic = ref dynamicRigidBodies[dynamicIndex];
                            ref var colliderDynamic  = ref dynamicColliders[dynamicIndex];

                            var aabbDynamic = colliderDynamic.ToAABB(transformDynamic.Position);

                            var manifold = CollisionManifold.CalculateManifold(aabbDynamic, aabbKinematic);

                            if (manifold.HasValue)
                            {
                                var impulse = manifold.Value.CalculateImpulse(rigidBodyDynamic, rigidBodyKinematic);
                                rigidBodyDynamic.AddImpulse(impulse);
                            }
                        }
                    }
                }
            }
        }

        public string Name => nameof(DynamicVsKinematicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}