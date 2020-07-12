using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    struct EnemyComponent : IEntityComponent
    {
        public PlayerTargetType enemyType;
    }
}