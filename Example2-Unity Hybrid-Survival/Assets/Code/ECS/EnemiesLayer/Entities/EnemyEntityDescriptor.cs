using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive.Enemies
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
            });
        }
    }
}