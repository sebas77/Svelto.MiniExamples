using System;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Physics.CollisionStructures;

namespace SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents
{
    public readonly struct CollisionManifoldEntityComponent : IEntityComponent
    {
        public static readonly CollisionManifoldEntityComponent Default = new CollisionManifoldEntityComponent();

        public static CollisionManifoldEntityComponent From(CollisionManifold collisionManifold)
        {
            return new CollisionManifoldEntityComponent(collisionManifold);
        }

        public readonly CollisionManifold? CollisionManifold;

        CollisionManifoldEntityComponent(CollisionManifold? collisionManifold)
        {
            CollisionManifold = collisionManifold;
        }

        bool Equals(CollisionManifoldEntityComponent other)
        {
            return Nullable.Equals(CollisionManifold, other.CollisionManifold)
                && CollisionManifold.Equals(other.CollisionManifold);
        }

        public override bool Equals(object obj)
        {
            return obj is CollisionManifoldEntityComponent other && Equals(other);
        }

        public override int GetHashCode() { return CollisionManifold.GetHashCode(); }
    }
}