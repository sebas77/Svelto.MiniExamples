using System;
using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive.Enemies
{
    static internal class EnemyGroups
    {
        public static readonly ExclusiveGroup EnemiesToRecycleGroups =
            new ExclusiveGroup((ushort)Enum.GetNames(typeof(PlayerTargetType)).Length);
    }

    namespace Svelto.ECS.Example.Survive.Enemies
    {
        public class EnemyTag: GroupTag<EnemyTag> { };
        public class DeadEnemies: GroupCompound<EnemyTag, Dead> { };
        public class AliveEnemies: GroupCompound<EnemyTag, Damageable> { };
    }
}