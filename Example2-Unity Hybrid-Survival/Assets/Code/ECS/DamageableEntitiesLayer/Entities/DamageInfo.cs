using UnityEngine;

namespace Svelto.ECS.Example.Survive.Damage
{
    public struct DamageInfo
    {
        internal int     damageToApply;
        public Vector3 damagePoint { get; }

        public DamageInfo(int damage, Vector3 point) : this()
        {
            damageToApply = damage;
            damagePoint   = point;
        }
    }
}