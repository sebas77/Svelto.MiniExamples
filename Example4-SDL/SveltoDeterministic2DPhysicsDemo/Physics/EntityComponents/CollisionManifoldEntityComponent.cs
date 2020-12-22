using System;
using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct CollisionManifoldEntityComponent : IEntityComponent, IEquatable<CollisionManifoldEntityComponent>
    {
        public CollisionManifold CollisionManifold;
        public RigidbodyEntityComponent LocalRigidBody;
        public RigidbodyEntityComponent CollisionRidigBody;

        public CollisionManifoldEntityComponent(CollisionManifold collisionManifold, ref RigidbodyEntityComponent localRigidBody, ref RigidbodyEntityComponent collisionRidigBody)
        {
            CollisionRidigBody = collisionRidigBody;
            LocalRigidBody = localRigidBody;
            CollisionManifold = collisionManifold;
        }

        public bool Equals(CollisionManifoldEntityComponent other)
        {
            return CollisionManifold.Equals(other.CollisionManifold) && LocalRigidBody.Equals(other.LocalRigidBody) && CollisionRidigBody.Equals(other.CollisionRidigBody);
        }

        public override bool Equals(object obj)
        {
            return obj is CollisionManifoldEntityComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CollisionManifold, LocalRigidBody, CollisionRidigBody);
        }
    }
}