using System;

namespace Svelto.ECS.Example.Survive
{
    [Serializable]
    public class JSonEnemyAttackData
    {
        public EnemyAttackData enemyAttackData;
        
        public JSonEnemyAttackData(EnemyAttackData attackData)
        {
            enemyAttackData = attackData;
        }
    }
    
    [Serializable]
    public class EnemyAttackData 
    {
        public float timeBetweenAttacks = 0.5f; // The time in seconds between each attack.
        public int   attackDamage       = 10;   // The amount of health taken away per attack.
    }
}