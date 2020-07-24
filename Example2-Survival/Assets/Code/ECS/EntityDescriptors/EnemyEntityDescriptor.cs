using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyEntityDescriptor : IEntityDescriptor
    {
        static readonly IComponentBuilder[] _componentsToBuild =
        {
            new ComponentBuilder<EnemyComponent>(),
            new ComponentBuilder<EnemyEntityViewComponent>(),
            new ComponentBuilder<EnemyAttackEntityViewComponent>(),
            new ComponentBuilder<HealthComponent>(),
            new ComponentBuilder<ScoreValueComponent>(),
            new ComponentBuilder<EnemyAttackComponent>(),
            new ComponentBuilder<DamageableComponent>(),
            new ComponentBuilder<EGIDTrackerViewComponent>(),
            new ComponentBuilder<DeathComponent>(),
        };

        public IComponentBuilder[] componentsToBuild => _componentsToBuild;
    }
}