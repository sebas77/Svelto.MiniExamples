using Svelto.ECS.Example.Survive.Characters.Sounds;

namespace Svelto.ECS.Example.Survive.Characters
{
    public class
        DamageableEntityDescriptor : GenericEntityDescriptor<DamageableComponent, HealthComponent, DeathComponent, DamageSoundEntityViewComponent> { }
}