using System;

namespace Svelto.ECS.Example.Survive.Damage
{
    public class Dead: GroupTag<Dead>
    {
//        static Dead()
//        {
//            bitmask = ExclusiveGroupBitmask.DISABLED_BIT;
//        }
    };

    public class Damageable : GroupTag<Damageable> { };
}