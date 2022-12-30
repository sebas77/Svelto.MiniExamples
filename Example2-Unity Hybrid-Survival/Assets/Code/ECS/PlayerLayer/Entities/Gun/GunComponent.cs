using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    public struct GunComponent : IEntityComponent
    {
        public float   timeBetweenBullets;
        public float   range;
        public int     damagePerShot;
        public float   timer;

        public bool fired;
    }
}