using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor: IEntityDescriptor
    {
        public IComponentBuilder[] componentsToBuild => staticComponents;

        readonly IComponentBuilder[] staticComponents = 
        {
            new ComponentBuilder<PositionEntityComponent>()
          , new ComponentBuilder<UECSEntityComponent>()
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