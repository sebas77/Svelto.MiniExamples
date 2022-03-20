using Svelto.ECS.MiniExamples.Turrets.EnemyLayer;
using Svelto.ECS.MiniExamples.Turrets.Player;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class GamePlayerBotEntityDescriptor : ExtendibleEntityDescriptor<PlayerBotEntityDescriptor>
    {
        public GamePlayerBotEntityDescriptor()
        {
            Add<TurretTargetComponent>();
        }
    }
}