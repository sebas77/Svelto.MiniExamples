using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DetectDynamicVsKinematicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            var kinematicEntities = entitiesDB
                   .QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(
                        GameGroups.KinematicRigidBodyWithBoxColliders.Groups);
            
            var dynamicEntities = entitiesDB
                   .QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent,
                        CollisionManifoldEntityComponent>(GameGroups.DynamicRigidBodyWithBoxColliders.Groups);

            // Detect dynamic vs kinematic collisions.
            foreach (var ((kinematicTransforms, kinematicRigidBodies, kinematicColliders, kinematicCount), _) in
                kinematicEntities)
            {
                for (var kinematicIndex = 0; kinematicIndex < kinematicCount; kinematicIndex++)
                {
                    ref var transformKinematic = ref kinematicTransforms[kinematicIndex];
                    ref var rigidBodyKinematic = ref kinematicRigidBodies[kinematicIndex];
                    ref var colliderKinematic  = ref kinematicColliders[kinematicIndex];

                    var aabbKinematic = colliderKinematic.ToAABB(transformKinematic.Position);

                    foreach (var ((dynamicTransforms, dynamicRigidBodies, dynamicColliders, dynamicCollisions,
                        dynamicCount), _) in dynamicEntities)
                    {
                        for (var dynamicIndex = 0; dynamicIndex < dynamicCount; dynamicIndex++)
                        {
                            ref var transformDynamic = ref dynamicTransforms[dynamicIndex];
                            ref var rigidBodyDynamic = ref dynamicRigidBodies[dynamicIndex];
                            ref var colliderDynamic  = ref dynamicColliders[dynamicIndex];
                            ref var dynamicCollision = ref dynamicCollisions[dynamicIndex];

                            var aabbDynamic = colliderDynamic.ToAABB(transformDynamic.Position);

                            var manifold =
                                CollisionManifold.CalculateManifold(rigidBodyDynamic, aabbDynamic, rigidBodyKinematic
                                                                  , aabbKinematic);

                            if (manifold.HasValue)
                            {
                                dynamicCollision.Collisions.Add(manifold.Value);
                            }
                        }
                    }
                }
            }
        }

        public string     Name       => nameof(DetectDynamicVsKinematicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }
    }
}