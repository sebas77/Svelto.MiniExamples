using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public struct PlayerInputDataComponent : IEntityComponent
    {
        public Vector3 input;
        public bool    fire;
    }
}