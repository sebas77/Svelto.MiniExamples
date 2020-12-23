using System;
using System.Runtime.CompilerServices;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures
{
    public struct CollisionManifold : IEquatable<CollisionManifold>
    {
        public static CollisionManifold? CalculateManifold(AABB a, AABB b)
        {
            // First, calculate the Minkowski difference. a maps to red, and b maps to blue from our example (though it doesn't matter!)
            var top    = a.Max.Y - b.Min.Y;
            var bottom = a.Min.Y - b.Max.Y;
            var left   = a.Min.X - b.Max.X;
            var right  = a.Max.X - b.Min.X;

            // If the Minkowski difference intersects the origin, there's a collision
            if (right < FixedPoint.Zero || left > FixedPoint.Zero || top < FixedPoint.Zero || bottom > FixedPoint.Zero)
                return null;

            // The pen vector is the shortest vector from the origin of the MD to an edge.
            // You know this has to be a vertical or horizontal line from the origin (these are by def. the shortest)
            var                min         = FixedPoint.MaxValue;
            FixedPointVector2? penetration = null;

            if (MathFixedPoint.Abs(left) < min)
            {
                min         = MathFixedPoint.Abs(left);
                penetration = FixedPointVector2.From(left, FixedPoint.Zero);
            }

            if (MathFixedPoint.Abs(right) < min)
            {
                min         = MathFixedPoint.Abs(right);
                penetration = FixedPointVector2.From(right, FixedPoint.Zero);
            }

            if (MathFixedPoint.Abs(top) < min)
            {
                min         = MathFixedPoint.Abs(top);
                penetration = FixedPointVector2.From(FixedPoint.Zero, top);
            }

            if (MathFixedPoint.Abs(bottom) < min)
            {
                min         = MathFixedPoint.Abs(bottom);
                penetration = FixedPointVector2.From(FixedPoint.Zero, bottom);
            }

            if (penetration.HasValue)
            {
                return new CollisionManifold(min, penetration.Value.Normalize());
            }

            return null;
        }

        public FixedPoint        Penetration;
        public FixedPointVector2 Normal;

        public CollisionManifold(FixedPoint penetration, FixedPointVector2 normal)
        {
            Penetration   = penetration;
            Normal        = normal;
        }

        public CollisionManifold Reverse()
        {
             return new CollisionManifold(-Penetration, -Normal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedPointVector2 CalculateImpulse(in RigidbodyEntityComponent rigidbodyA
                                                , in RigidbodyEntityComponent rigidbodyB)
        {
            // Calculate relative velocity
            var rv = rigidbodyB.Velocity - rigidbodyA.Velocity;

            // Calculate relative velocity in terms of the normal direction
            var velAlongNormal = FixedPointVector2.Dot(rv, Normal);

            // Do not resolve if velocities are separating
            if (velAlongNormal > FixedPoint.Zero)
                return FixedPointVector2.Zero;

            // Calculate restitution
            var e = MathFixedPoint.Min(rigidbodyA.Restitution, rigidbodyB.Restitution);

            // Calculate impulse scalar
            var j = -(FixedPoint.One + e) * velAlongNormal;
            //j /= rigidbody.InverseMass + collisionTarget.InverseMass;

            // Apply impulse
            return Normal * j;
        }

        public bool Equals(CollisionManifold other)
        {
            return Penetration.Equals(other.Penetration)
                && Normal.Equals(other.Normal);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Penetration, Normal);
        }
    }
}