using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;


namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public PlayerEntityDescriptor()
        {
            ExtendWith<RBEntityDescriptor>();
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<GameObjectEntityComponent>()
              , new ComponentBuilder<PlayerInputDataComponent>()
              , new ComponentBuilder<AnimationComponent>()
              , new ComponentBuilder<WeaponComponent>()
              , new ComponentBuilder<CameraReferenceComponent>()
            });
        }
    }
}
