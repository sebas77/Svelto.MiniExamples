using System;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public interface IEnemyMovementComponent
    {
        bool    navMeshEnabled      { set; }
        Vector3 navMeshDestination  { set; }
        bool    setCapsuleAsTrigger { set; }
    }

    public interface IEnemyTriggerComponent
    {
        DispatchOnChange<EnemyCollisionData> hitChange { get; set; }
    }

    public struct EnemyCollisionData : IEquatable<EnemyCollisionData>
    {
        public          EGID otherEntityID;
        public readonly bool collides;

        public EnemyCollisionData(EGID otherEntityID, bool collides)
        {
            this.otherEntityID = otherEntityID;
            this.collides      = collides;
        }

        public bool Equals(EnemyCollisionData other)
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

    public interface IEnemyVFXComponent
    {
        Vector3 position { set; }
        bool    play     { set; }
    }
}