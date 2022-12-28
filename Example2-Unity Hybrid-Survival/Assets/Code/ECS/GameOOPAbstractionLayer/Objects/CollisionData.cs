using System;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct CollisionData: IEquatable<CollisionData>
    {
        public EntityReference otherEntityID;
        public readonly bool collides;

        public CollisionData(EntityReference otherEntityID, bool collides)
        {
            this.otherEntityID = otherEntityID;
            this.collides = collides;
        }

        public bool Equals(CollisionData other)
        {
            return otherEntityID.Equals(other.otherEntityID) && collides == other.collides;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (otherEntityID.GetHashCode() * 397) ^ collides.GetHashCode();
            }
        }
    }
}