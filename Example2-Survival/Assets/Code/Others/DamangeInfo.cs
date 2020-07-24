using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public struct DamageInfo
    {
        public readonly int     damageToApply;
        public Vector3 damagePoint { get; }

        public DamageInfo(int damage, Vector3 point) : this()
        {
            damageToApply = damage;
            damagePoint   = point;
        }
    }
}