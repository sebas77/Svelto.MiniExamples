using System;
using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct CollisionManifoldEntityComponent : IEntityComponent, IEquatable<CollisionManifoldEntityComponent>
    {
        public static CollisionManifoldEntityComponent From(CollisionManifold collisionManifold)
        {
            return new CollisionManifoldEntityComponent(collisionManifold);
        }

        public CollisionManifold? CollisionManifold;

        CollisionManifoldEntityComponent(CollisionManifold? collisionManifold)
        {
            CollisionManifold = collisionManifold;
        }

        public bool Equals(CollisionManifoldEntityComponent other)
        {
            return Nullable.Equals(CollisionManifold, other.CollisionManifold);
        }
    }
}