using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public EnemyEntityDescriptor()
        {
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<EnemyComponent>(),
                new ComponentBuilder<EnemyEntityViewComponent>(),
                new ComponentBuilder<ScoreValueComponent>(),
                new ComponentBuilder<EnemyAttackComponent>(),
                new ComponentBuilder<EGIDTrackerViewComponent>()
            });
        }
    }
}