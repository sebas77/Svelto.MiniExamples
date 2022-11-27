using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;
using Svelto.ECS.Example.Survive.Transformable;

namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public PlayerEntityDescriptor()
        {
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<PlayerEntityComponent>(),
                new ComponentBuilder<PositionComponent>()
              , new ComponentBuilder<EnemyTargetEntityViewComponent>()
              , new ComponentBuilder<PlayerInputDataComponent>()
              , new ComponentBuilder<AnimationComponent>()
              , new ComponentBuilder<WeaponComponent>()
              , new ComponentBuilder<SpeedComponent>()
              , new ComponentBuilder<CameraReferenceComponent>()
            });
        }
    }
}
