namespace Svelto.ECS.Example.Survive.Enemies
{
    public struct EnemyAttackComponent: IEntityComponent
    {
        public float timeBetweenAttack; // The time in seconds between each attack.
        public int attackDamage;        // The amount of health taken away per attack.
        public float timer;
    }
}