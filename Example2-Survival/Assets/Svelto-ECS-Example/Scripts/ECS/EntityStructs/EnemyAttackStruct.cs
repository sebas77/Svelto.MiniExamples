namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct EnemyAttackStruct:IEntityStruct
    {
        public EnemyCollisionData    entityInRange;
        public float                 timeBetweenAttack; // The time in seconds between each attack.
        public int                   attackDamage ;   // The amount of health taken away per attack.
        public float                 timer;
        
        public EGID ID { get; set; }
    }
}