using System;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DetectDynamicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            var dynamicEntities = new DoubleEntitiesEnumerator<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent, CollisionManifoldEntityComponent>(
                entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent, CollisionManifoldEntityComponent>(
                    GameGroups.DynamicRigidBodyWithBoxColliders.Groups));

            foreach (var ((transformsA, rigidBodiesA, collidersA, collisionManifoldsA, _), indexA, (transformsB, rigidBodiesB, collidersB, collisionManifoldsB, _), indexB) in dynamicEntities)
            {
                ref var colliderA  = ref collidersA[indexA];
                ref var transformA = ref transformsA[indexA];
                ref var rigidBodyA = ref rigidBodiesA[indexA];
                ref var collisionManifoldA = ref collisionManifoldsA[indexA];

                var aabbA = colliderA.ToAABB(transformA.Position);

                ref var colliderB  = ref collidersB[indexB];
                ref var transformB = ref transformsB[indexB];
                ref var rigidBodyB = ref rigidBodiesB[indexB];
                ref var collisionManifoldB = ref collisionManifoldsB[indexB];

                var aabbB = colliderB.ToAABB(transformB.Position);

                var manifold = CollisionManifold.CalculateManifold(rigidBodyA, aabbA, rigidBodyB, aabbB);

                if (manifold.HasValue)
                {
                    collisionManifoldA.Collisions.Add(manifold.Value);
                    collisionManifoldB.Collisions.Add(manifold.Value.Reverse());
                }
            }
        }

        public string Name => nameof(DetectDynamicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}