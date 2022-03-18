using Svelto.ECS.Example.Survive.Sounds;

namespace Svelto.ECS.Example.Survive
{
    public class
        DamageableEntityDescriptor : GenericEntityDescriptor<DamageableComponent, HealthComponent, DeathComponent,
            DamageSoundEntityViewComponent>
    {
    }
}