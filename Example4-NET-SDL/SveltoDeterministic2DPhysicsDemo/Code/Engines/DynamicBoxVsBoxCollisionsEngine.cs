using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DynamicBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(in FixedPoint delta)
        {
            var dynamicEntities = new DoubleEntitiesEnumerator<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(
                entitiesDB.QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, BoxColliderEntityComponent>(
                    GameGroups.DynamicRigidBodyWithBoxColliders.Groups));

            foreach (var ((transformsA, rigidBodiesA, collidersA, _), indexA,
                          (transformsB, rigidBodiesB, collidersB, _), indexB) in dynamicEntities)
            {
                ref var colliderA  = ref collidersA[indexA];
                ref var transformA = ref transformsA[indexA];
                ref var rigidBodyA = ref rigidBodiesA[indexA];

                var aabbA = colliderA.ToAABB(transformA.Position);

                ref var colliderB  = ref collidersB[indexB];
                ref var transformB = ref transformsB[indexB];
                ref var rigidBodyB = ref rigidBodiesB[indexB];

                var aabbB = colliderB.ToAABB(transformB.Position);

                var manifold = CollisionManifold.CalculateManifold(aabbA, aabbB);

                if (manifold.HasValue)
                {
                    var impulse = manifold.Value.CalculateImpulse(rigidBodyA, rigidBodyB);

                    rigidBodyA.AddImpulse(impulse);
                    rigidBodyB.AddImpulse(-impulse);
                }
            }
        }

        public string Name => nameof(DynamicBoxVsBoxCollisionsEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}