using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor: IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild { get; } =
        {
            new ComponentBuilder<PositionEntityComponent>()
          , new ComponentBuilder<DOTSEntityComponent>()
          , new ComponentBuilder<SpawnPointEntityComponent>()
          , new ComponentBuilder<VelocityEntityComponent>()
          , new ComponentBuilder<SpeedEntityComponent>()
          , new ComponentBuilder<EGIDComponent>()
          , new ComponentBuilder<MealInfoComponent>()
        };
    }

    public struct MealInfoComponent : IEntityComponent
    {
        public EGID targetMeal;
    }
}