using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public struct GunAttributesComponent : IEntityComponent
    {
        public float   timeBetweenBullets;
        public float   range;
        public int     damagePerShot;
        public float   timer;
        public Vector3 lastTargetPosition;
    }
}