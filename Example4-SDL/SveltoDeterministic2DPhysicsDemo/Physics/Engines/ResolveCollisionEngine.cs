using System;
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
            foreach (var ((manifolds, impulses, count), _) in entitiesDB
               .QueryEntities<CollisionManifoldEntityComponent, ImpulseEntityComponent>(
                    GameGroups.DynamicBoxVsDynamicBoxInCollision.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    ref var manifold = ref manifolds[i];
                    ref var impulse = ref impulses[i];

                    impulse.Impulse = CalculateImpulse(ref manifold.CollisionManifold, ref manifold.LocalRigidBody, ref manifold.CollisionRidigBody);
                }
            }

            foreach (var (( manifolds, impulses, count), _) in entitiesDB
                .QueryEntities<CollisionManifoldEntityComponent, ImpulseEntityComponent>(
                    GameGroups.DynamicBoxVsKinematicBoxInCollision.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    ref var manifold = ref manifolds[i];
                    ref var impulse = ref impulses[i];

                    impulse.Impulse = CalculateImpulse(ref manifold.CollisionManifold, ref manifold.LocalRigidBody, ref manifold.CollisionRidigBody);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static FixedPointVector2 CalculateImpulse(ref CollisionManifold manifold
                                                , ref RigidbodyEntityComponent rigidbodyA
                                                , ref RigidbodyEntityComponent rigidbodyB)
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