using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player
{
    public struct PlayerInputDataComponent : IEntityComponent
    {
        public Vector3 input;
    }
}