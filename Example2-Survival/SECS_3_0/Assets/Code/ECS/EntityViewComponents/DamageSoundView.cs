using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Characters.Sounds
{
    public struct DamageSoundEntityView : IEntityViewComponent
    {
        public IDamageSoundComponent audioComponent;
        public EGID                  ID { get; set; }
    }
}