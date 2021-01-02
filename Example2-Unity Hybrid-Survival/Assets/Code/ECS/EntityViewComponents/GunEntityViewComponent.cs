using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public struct GunEntityViewComponent : IEntityViewComponent
    {
        public IGunFXComponent gunFXComponent;
        
        public EGID            ID { get; set; }
    }
}