using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using MiniExamples.DeterministicPhysicDemo.Physics.Types;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ResolveCollisionEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            // var boxRigidbodies    = new NB<RigidbodyEntityComponent>();
            // var boxManifolds      = new NB<CollisionManifoldEntityComponent>();
            // var boxCount          = 0;
            // var circleRigidbodies = new NB<RigidbodyEntityComponent>();
            // var circleManifolds   = new NB<CollisionManifoldEntityComponent>();
            // var circleCount       = 0;
            //
            // foreach (var ((rigidbodies, manifolds, count), _) in entitiesDB
            //    .QueryEntities<RigidbodyEntityComponent, CollisionManifoldEntityComponent>(
            //         GameGroups.RigidBodyWithBoxColliders.Groups))
            // {
            //     boxRigidbodies = rigidbodies;
            //     boxManifolds   = manifolds;
            //     boxCount       = count;
            // }
            //
            // foreach (var ((rigidbodies, manifolds, count), _) in entitiesDB
            //    .QueryEntities<RigidbodyEntityComponent, CollisionManifoldEntityComponent>(
            //         GameGroups.RigidBodyWithCircleColliders.Groups))
            // {
            //     circleRigidbodies = rigidbodies;
            //     circleManifolds   = manifolds;
            //     circleCount       = count;
            // }
            //
            // for (var i = 0; i < boxCount; i++)
            // {
            //     ref var manifold = ref boxManifolds[i];
            //
            //     if (!manifold.CollisionManifold.HasValue)
            //         continue;
            //
            //     if (manifold.CollisionManifold.Value.CollisionType == CollisionType.AABBToCircle)
            //         ResolveCollision(i, manifold.CollisionManifold.Value
            //                        , ref boxRigidbodies[manifold.CollisionManifold.Value.EntityIndex1]
            //                        , ref circleRigidbodies[manifold.CollisionManifold.Value.EntityIndex2]);
            //     else
            //         ResolveCollision(i, manifold.CollisionManifold.Value
            //                        , ref boxRigidbodies[manifold.CollisionManifold.Value.EntityIndex1]
            //                        , ref boxRigidbodies[manifold.CollisionManifold.Value.EntityIndex2]);
            // }
            //
            // for (var i = 0; i < circleCount; i++)
            // {
            //     ref var manifold = ref circleManifolds[i];
            //
            //     if (!manifold.CollisionManifold.HasValue)
            //         continue;
            //
            //     if (manifold.CollisionManifold.Value.CollisionType == CollisionType.AABBToCircle)
            //         ResolveCollision(i, manifold.CollisionManifold.Value
            //                        , ref boxRigidbodies[manifold.CollisionManifold.Value.EntityIndex1]
            //                        , ref circleRigidbodies[manifold.CollisionManifold.Value.EntityIndex2]);
            //     else
            //         ResolveCollision(i, manifold.CollisionManifold.Value
            //                        , ref circleRigidbodies[manifold.CollisionManifold.Value.EntityIndex1]
            //                        , ref circleRigidbodies[manifold.CollisionManifold.Value.EntityIndex2]);
            // }
        }

        public   void             Ready() { }
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ResolveCollisionEngine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ResolveCollision(int index, CollisionManifold manifold, ref RigidbodyEntityComponent rigidbodyA
                                   , ref RigidbodyEntityComponent rigidbodyB)
        {
            // Calculate relative velocity
            var rv = rigidbodyB.Velocity - rigidbodyA.Velocity;

            // Calculate relative velocity in terms of the normal direction
            var velAlongNormal = FixedPointVector2.Dot(rv, manifold.Normal);

            // Do not resolve if velocities are separating
            if (velAlongNormal > FixedPoint.Zero)
                return;

            // Calculate restitution
            var e = MathFixedPoint.Min(rigidbodyA.Restitution, rigidbodyB.Restitution);

            // Calculate impulse scalar
            var j = -(FixedPoint.One + e) * velAlongNormal;
            //j /= rigidbody.InverseMass + collisionTarget.InverseMass;

            // Apply impulse
            var impulse = manifold.Normal * j;

            if (manifold.CollisionType == CollisionType.AABBToCircle)
            {
                if (manifold.EntityIndex1 == index)
                    rigidbodyA.Direction = (rigidbodyA.Velocity - impulse).Normalize();
                else
                    rigidbodyB.Direction = (rigidbodyB.Velocity + impulse).Normalize();
            }
            else
            {
                if (!rigidbodyA.IsKinematic)
                    rigidbodyA.Direction = (rigidbodyA.Velocity - impulse).Normalize();

                if (!rigidbodyB.IsKinematic)
                    rigidbodyB.Direction = (rigidbodyB.Velocity + impulse).Normalize();
            }
        }
    }
}