using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Sounds
{
    public struct DamageSoundEntityViewComponent : IEntityViewComponent
    {
        public IDamageSoundComponent audioComponent;
        
        public EGID                  ID { get; set; }
    }
}