using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public struct DamageInfo
    {
        public int shotDamageToApply;
        public Vector3 damagePoint { get; }
        
        public DamageInfo(int shotDamage, Vector3 point) : this()
        {
            shotDamageToApply = shotDamage;
            damagePoint = point;
        }
    }
}
    
