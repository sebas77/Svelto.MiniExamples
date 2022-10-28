using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Damage
{
    public struct DamageSoundEntityComponent : IEntityComponent
    {
        public AudioType playOneShot;
    }
}