using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public PlayerEntityDescriptor()
        {
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<PlayerEntityViewComponent>()
              , new ComponentBuilder<CameraTargetEntityViewComponent>()
              , new ComponentBuilder<EnemyTargetEntityViewComponent>()
              , new ComponentBuilder<PlayerInputDataComponent>()
              , new ComponentBuilder<EGIDTrackerViewComponent>()
              , new ComponentBuilder<EGIDComponent>()
              , new ComponentBuilder<SpeedComponent>()
            });
        }
    }
}
