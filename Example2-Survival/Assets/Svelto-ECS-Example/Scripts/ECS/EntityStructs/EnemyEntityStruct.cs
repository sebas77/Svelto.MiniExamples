using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    struct EnemyEntityStruct:IEntityStruct
    {
        public PlayerTargetType enemyType;
        public EGID ID { get; set; }
    }
}