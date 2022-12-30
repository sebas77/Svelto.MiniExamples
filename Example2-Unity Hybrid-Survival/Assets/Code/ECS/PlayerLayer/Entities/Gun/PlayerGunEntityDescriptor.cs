using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Player.Gun;

namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerGunEntityDescriptor: GenericEntityDescriptor<GameObjectEntityComponent, GunOOPEntityComponent,
        GunComponent> { }
}