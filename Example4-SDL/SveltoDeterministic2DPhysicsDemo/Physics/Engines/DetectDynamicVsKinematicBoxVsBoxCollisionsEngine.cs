using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.Descriptors;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DetectDynamicVsKinematicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public DetectDynamicVsKinematicBoxVsBoxCollisionsEngine(IEntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
        }

        public void Execute(FixedPoint delta)
        {
            var kinematicEntities = entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(GameGroups.KinematicRigidBodyWithBoxColliders.Groups);
            var dynamicEntities   = entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent, EGIDComponent>(GameGroups.DynamicRigidBodyWithBoxColliders.Groups);

            // Detect dynamic vs kinematic collisions.
            foreach (var ((kinematicTransforms, kinematicRigidBodies, kinematicColliders, kinematicCount), _) in kinematicEntities)
            {
                for (var kinematicIndex = 0; kinematicIndex < kinematicCount; kinematicIndex++)
                {
                    ref var colliderKinematic  = ref kinematicColliders[kinematicIndex];
                    ref var transformKinematic = ref kinematicTransforms[kinematicIndex];
                    ref var kinematicRigidBody = ref kinematicRigidBodies[kinematicIndex];

                    var aabbKinematic = colliderKinematic.ToAABB(transformKinematic.Position);

                    foreach (var ((dynamicTransforms, dynamicRidigBodies, dynamicColliders, dynamicEgids, dynamicCount), _) in dynamicEntities)
                    {
                        for (var dynamicIndex = 0; dynamicIndex < dynamicCount; dynamicIndex++)
                        {
                            ref var colliderDynamic  = ref dynamicColliders[dynamicIndex];
                            ref var transformDynamic = ref dynamicTransforms[dynamicIndex];
                            ref var rigidBodyDynamic = ref dynamicRidigBodies[dynamicIndex];
                            ref var dynamicEgid = ref dynamicEgids[dynamicIndex];

                            var aabbDynamic = colliderDynamic.ToAABB(transformDynamic.Position);

                            var manifold = CollisionManifold.CalculateManifold(dynamicIndex, aabbDynamic, kinematicIndex, aabbKinematic);

                            if (manifold.HasValue)
                            {
                                var initializerA = _entityFactory.BuildEntity<CollisionDescriptor>(
                                    EgidFactory.GetNextId(), GameGroups.DynamicBoxVsKinematicBoxInCollision.BuildGroup);

                                initializerA.Init(new ReferenceEgidComponent(dynamicEgid.ID));
                                initializerA.Init(new CollisionManifoldEntityComponent(manifold.Value, ref rigidBodyDynamic, ref kinematicRigidBody));
                                initializerA.Init(new ImpulseEntityComponent(FixedPointVector2.Zero));
                            }
                        }
                    }
                }
            }
        }

        readonly IEntityFactory _entityFactory;

        public string Name => nameof(DetectDynamicVsKinematicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}