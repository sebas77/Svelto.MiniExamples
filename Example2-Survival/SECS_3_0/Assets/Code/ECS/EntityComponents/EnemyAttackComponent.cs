namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyAttackComponent : IEntityComponent
    {
        public EnemyCollisionData entityInRange;
        public float              timeBetweenAttack; // The time in seconds between each attack.
        public int                attackDamage;      // The amount of health taken away per attack.
        public float              timer;
    }
}