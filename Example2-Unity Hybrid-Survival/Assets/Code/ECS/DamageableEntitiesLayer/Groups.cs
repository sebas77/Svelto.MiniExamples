namespace Svelto.ECS.Example.Survive
{
    public class isDying : GroupTag<Dead> { };
    public class Dead : GroupTag<Dead> { };

    public class Damageable : GroupTag<Damageable> { };
}