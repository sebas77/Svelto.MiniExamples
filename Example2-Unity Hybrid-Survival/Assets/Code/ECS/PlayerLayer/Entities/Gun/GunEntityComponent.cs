using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    public struct GunEntityComponent : IEntityComponent
    {
        public Ray     shootRay;
        public float   effectsDisplayTime;
        public Vector3 lineEndPosition;
        public Vector3 lineStartPosition;
        public bool    lineEnabled;
        public bool    play;
        public bool    lightEnabled;
        public bool    playAudio;
    }
}