using System;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public interface ISpawnedComponent
    {
        bool spawned {get; set;}
    }


    public interface IAmmoTriggerComponent
    {
        DispatchOnChange<AmmoCollisionData> hitChange { get; set; }
    }

    public struct AmmoCollisionData : IEquatable<AmmoCollisionData>
    {
        public          EntityReference otherEntityID;
        public readonly bool collides;

        public AmmoCollisionData(EntityReference otherEntityID, bool collides)
        {
            this.otherEntityID = otherEntityID;
            this.collides      = collides;
        }

        public bool Equals(AmmoCollisionData other)
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