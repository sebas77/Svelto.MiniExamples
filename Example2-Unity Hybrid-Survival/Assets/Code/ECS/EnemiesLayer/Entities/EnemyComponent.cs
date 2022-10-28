using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive.Enemies
{
    struct EnemyComponent : IEntityComponent
    {
        public PlayerTargetType enemyType;
    }
}