using System;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.Types;

namespace MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures
{
    public readonly struct CollisionManifold
    {
        public readonly FixedPoint        Penetration;
        public readonly FixedPointVector2 Normal;
        public readonly CollisionType     CollisionType;
        public readonly int               EntityIndex1;
        public readonly int               EntityIndex2;

        public CollisionManifold(FixedPoint penetration, FixedPointVector2 normal, CollisionType collisionType, 
                          int entityIndex1, int entityIndex2)
        {
            Penetration   = penetration;
            Normal        = normal;
            CollisionType = collisionType;
            EntityIndex1  = entityIndex1;
            EntityIndex2  = entityIndex2;
        }

        // public CollisionManifold Reverse()
        // {
        //     return new CollisionManifold(-Penetration, -Normal, CollisionType, EntityIndex1, EntityIndex2);
        // }

        bool Equals(CollisionManifold other)
        {
            return Penetration.Equals(other.Penetration) && Normal.Equals(other.Normal)
                                                         && CollisionType == other.CollisionType
                                                         && EntityIndex1 == other.EntityIndex1
                                                         && EntityIndex2 == other.EntityIndex2;
        }

        public override bool Equals(object obj) { return obj is CollisionManifold other && Equals(other); }

        public override int GetHashCode()
        {
            return HashCode.Combine(Penetration, Normal, (int) CollisionType, EntityIndex1, EntityIndex2);
        }
    }
}