using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyEntityDescriptor : ExtendibleEntityDescriptor<DamageableEntityDescriptor>
    {
        public EnemyEntityDescriptor()
        {
            Add<PositionComponent>();
            ExtendWith(new IComponentBuilder[]
            {
                new ComponentBuilder<GameObjectEntityComponent>(),
                new ComponentBuilder<EnemyComponent>(),
                new ComponentBuilder<NavMeshComponent>(),
                new ComponentBuilder<ScoreValueComponent>(),
                new ComponentBuilder<EnemyAttackComponent>(),
                new ComponentBuilder<AnimationComponent>(),
                new ComponentBuilder<VFXComponent>(),
                new ComponentBuilder<CollisionComponent>()
            });
        }
    }
}