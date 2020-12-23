using System.Runtime.CompilerServices;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ResolveCollisionEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            var entities = entitiesDB
                .QueryEntities<CollisionManifoldEntityComponent, RigidbodyEntityComponent, ImpulseEntityComponent>(
                    GameGroups.DynamicRigidBodyWithBoxColliders.Groups);

            foreach (var ((manifolds, rigidBodies, impulses, count), _) in entities)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var manifold = ref manifolds[i];
                    ref var impulse = ref impulses[i];

                    for (var j = 0; j < manifold.Collisions.count; j++)
                    {
                        ref var collision = ref manifold.Collisions[j];

                        var calculatedImpulse = CalculateImpulse(collision, collision.LocalRigidBody,
                                                                 collision.RemoteRigidBody);

                        impulse.Impulses.Add(calculatedImpulse);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static FixedPointVector2 CalculateImpulse(in CollisionManifold manifold, in RigidbodyEntityComponent rigidbodyA
                                                                               , in RigidbodyEntityComponent rigidbodyB)
        {
            // Calculate relative velocity
            var rv = rigidbodyB.Velocity - rigidbodyA.Velocity;

            // Calculate relative velocity in terms of the normal direction
            var velAlongNormal = FixedPointVector2.Dot(rv, manifold.Normal);

            // Do not resolve if velocities are separating
            if (velAlongNormal > FixedPoint.Zero)
                return FixedPointVector2.Zero;

            // Calculate restitution
            var e = MathFixedPoint.Min(rigidbodyA.Restitution, rigidbodyB.Restitution);

            // Calculate impulse scalar
            var j = -(FixedPoint.One + e) * velAlongNormal;
            //j /= rigidbody.InverseMass + collisionTarget.InverseMass;

            // Apply impulse
            return manifold.Normal * j;
        }

        public string Name => nameof(ResolveCollisionEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}