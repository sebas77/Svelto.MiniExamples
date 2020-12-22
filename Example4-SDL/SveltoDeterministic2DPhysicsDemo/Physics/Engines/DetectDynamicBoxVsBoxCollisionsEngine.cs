using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.Descriptors;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DetectDynamicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public DetectDynamicBoxVsBoxCollisionsEngine(IEntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
        }

        public void Execute(FixedPoint delta)
        {
            var dynamicEntities = new DoubleEntitiesEnumerator<TransformEntityComponent, BoxColliderEntityComponent, RigidbodyEntityComponent, EGIDComponent>(
                entitiesDB.QueryEntities<TransformEntityComponent, BoxColliderEntityComponent, RigidbodyEntityComponent, EGIDComponent>(
                    GameGroups.DynamicRigidBodyWithBoxColliders.Groups));

            foreach (var ((transformsA, collidersA, rigidBodiesA, egidsA, _), indexA, (transformsB, collidersB, rigidBodiesB, egidsB, _), indexB) in dynamicEntities)
            {
                ref var colliderA  = ref collidersA[indexA];
                ref var transformA = ref transformsA[indexA];
                ref var rigidBodyA = ref rigidBodiesA[indexA];
                ref var egidA = ref egidsA[indexA];

                var aabbA = colliderA.ToAABB(transformA.Position);

                ref var colliderB  = ref collidersB[indexB];
                ref var transformB = ref transformsB[indexB];
                ref var rigidBodyB = ref rigidBodiesB[indexB];
                ref var egidB = ref egidsA[indexB];

                var aabbB = colliderB.ToAABB(transformB.Position);

                var manifold = CollisionManifold.CalculateManifold(indexA, aabbA, indexB, aabbB);

                if (manifold.HasValue)
                {
                    var initializerA = _entityFactory.BuildEntity<CollisionDescriptor>(
                        EgidFactory.GetNextId(), GameGroups.DynamicBoxVsDynamicBoxInCollision.BuildGroup);
                    initializerA.Init(new ReferenceEgidComponent(egidA.ID));
                    initializerA.Init(new CollisionManifoldEntityComponent(manifold.Value, ref rigidBodyA, ref rigidBodyB));
                    initializerA.Init(new ImpulseEntityComponent(FixedPointVector2.Zero));

                    var initializerB = _entityFactory.BuildEntity<CollisionDescriptor>(
                        EgidFactory.GetNextId(), GameGroups.DynamicBoxVsDynamicBoxInCollision.BuildGroup);
                    initializerB.Init(new ReferenceEgidComponent(egidB.ID));
                    initializerB.Init(new CollisionManifoldEntityComponent(manifold.Value.Reverse(), ref rigidBodyB, ref rigidBodyA));
                    initializerB.Init(new ImpulseEntityComponent(FixedPointVector2.Zero));
                }
            }
        }

        readonly IEntityFactory _entityFactory;

        public string Name => nameof(DetectDynamicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}