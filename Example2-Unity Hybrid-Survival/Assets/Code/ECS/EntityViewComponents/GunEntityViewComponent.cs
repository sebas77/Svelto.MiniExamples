using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    public struct GunEntityViewComponent : IEntityViewComponent
    {
        public IGunFXComponent gunFXComponent;
    }
}