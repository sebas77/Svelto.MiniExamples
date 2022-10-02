namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerTag : GroupTag<PlayerTag> { };
    public class Player : GroupCompound<PlayerTag, Damageable> { };
}