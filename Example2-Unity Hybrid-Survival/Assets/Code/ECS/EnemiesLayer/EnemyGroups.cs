using Svelto.ECS.Example.Survive.Damage;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class Enemy: GroupTag<Enemy> { };
    
    public class EnemyDeadGroup: GroupCompound<Enemy, Dead> { };
    public class EnemyAliveGroup: GroupCompound<Enemy, Damageable> { };
    
    public class EnemyTarget : GroupTag<EnemyTarget> { };
}