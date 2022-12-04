using System;
using Svelto.ECS.Example.Survive.Damage;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyTag: GroupTag<EnemyTag> { };
    public class DeadEnemies: GroupCompound<EnemyTag, Dead> { };
    public class AliveEnemies: GroupCompound<EnemyTag, DamageableTag> { };
    
    public class EnemyTarget : GroupTag<EnemyTarget> { };
}