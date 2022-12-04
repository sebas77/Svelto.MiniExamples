using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public EnemyEntityDescriptor()
        {
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<GameObjectEntityComponent>(),
                new ComponentBuilder<EnemyComponent>(),
                new ComponentBuilder<EnemyEntityViewComponent>(),
                new ComponentBuilder<ScoreValueComponent>(),
                new ComponentBuilder<EnemyAttackComponent>(),
            });
        }
    }
}