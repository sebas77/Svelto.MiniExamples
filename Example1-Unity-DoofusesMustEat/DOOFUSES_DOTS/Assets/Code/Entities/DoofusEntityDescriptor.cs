using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    class DoofusEntityDescriptor: IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild { get; } =
        {
            new ComponentBuilder<PositionEntityComponent>()
          , new ComponentBuilder<DOTSEntityComponent>()
          , new ComponentBuilder<VelocityEntityComponent>()
          , new ComponentBuilder<SpeedEntityComponent>()
          , new ComponentBuilder<MealTargetComponent>()
        };
    }

    public struct MealTargetComponent : IEntityComponent
    {
        public EGID targetMeal;
    }
}