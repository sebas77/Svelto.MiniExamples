using Svelto.DataStructures.Experimental;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    public struct PlayerEntityComponent : IEntityComponent
    {
        public bool isKinematic;
        public Vector3 velocity;
        public Vector3 position;
        public Quaternion rotation;

        public ValueIndex resourceIndex;
    }
}
