using Svelto.ECS.Example.Survive.Sounds;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Damage
{
    public struct DamageSoundEntityViewComponent : IEntityViewComponent
    {
        public ISoundComponent audioComponent;
        
        public EGID                  ID { get; set; }
    }
}