using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Sounds
{
    public struct DamageSoundEntityComponent : IEntityComponent
    {
        public AudioType playOneShot;
    }
}