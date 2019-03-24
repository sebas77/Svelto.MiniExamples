using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public struct PlayerInputDataStruct : IEntityStruct
    {
        public Vector3 input;
        public Ray     camRay;
        public bool    fire;
        public EGID    ID { get; set; }
    }
}