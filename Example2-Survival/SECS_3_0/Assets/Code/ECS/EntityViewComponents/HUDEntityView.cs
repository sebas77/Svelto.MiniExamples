using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.HUD
{
    public struct HUDEntityView : IEntityViewComponent
    {
        public IAnimationComponent    HUDAnimator;
        public IDamageHUDComponent    damageImageComponent;
        public IHealthSliderComponent healthSliderComponent;
        public IScoreComponent        scoreComponent;
        public EGID                   ID { get; set; }
    }
}