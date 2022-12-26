using Svelto.ECS.Example.Survive.Damage;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class Enemy: GroupTag<Enemy> { };
    
    public class DeadEnemiesGroup: GroupCompound<Enemy, Dead> { };
    public class EnemiesGroup: GroupCompound<Enemy, Damageable> { };
    
    public class EnemyTarget : GroupTag<EnemyTarget> { };
}