using System;
using Svelto.ECS.Example.Survive.Damage;

namespace Svelto.ECS.Example.Survive.Enemies
{
    static class EnemyGroups
    {
        public static readonly ExclusiveGroup EnemiesToRecycleGroups =
            new ExclusiveGroup((ushort)Enum.GetNames(typeof(PlayerTargetType)).Length);
    }

    public class EnemyTag: GroupTag<EnemyTag> { };
    public class DeadEnemies: GroupCompound<EnemyTag, Dead> { };
    public class AliveEnemies: GroupCompound<EnemyTag, DamageableTag> { };
}