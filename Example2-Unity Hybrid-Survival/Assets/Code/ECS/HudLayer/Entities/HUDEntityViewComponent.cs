using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.HUD
{
    public struct HUDEntityViewComponent : IEntityViewComponent
    {
        public IDamageHUDComponent    damageHUDComponent;
        public IHealthSliderComponent healthSliderComponent;
        public IScoreComponent        scoreComponent;
    }
}