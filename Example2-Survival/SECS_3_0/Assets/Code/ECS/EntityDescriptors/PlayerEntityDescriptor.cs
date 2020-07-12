using Svelto.ECS.Example.Survive.Camera;
using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerEntityDescriptor : IEntityDescriptor
    {
        static readonly IComponentBuilder[] _componentsToBuild =
        {
            new ComponentBuilder<PlayerEntityViewComponent>(),
            new ComponentBuilder<DamageableComponent>(),
            new ComponentBuilder<DamageSoundEntityView>(),
            new ComponentBuilder<CameraTargetEntityView>(),
            new ComponentBuilder<HealthComponent>(),
            new ComponentBuilder<DeathComponent>(),
            new ComponentBuilder<EnemyTargetEntityViewComponent>(),
            new ComponentBuilder<PlayerInputDataComponent>(),
            new ComponentBuilder<EGIDTrackerViewComponent>()
        };

        public IComponentBuilder[] componentsToBuild => _componentsToBuild;
    }
}