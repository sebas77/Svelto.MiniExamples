//ing Svelto.ECS.Example.Survive.Sounds;

using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Damage
{
    public class
        DamageableEntityDescriptor: GenericEntityDescriptor<DamageableComponent, HealthComponent, DeathComponent,
            DamageSoundComponent> { }
}