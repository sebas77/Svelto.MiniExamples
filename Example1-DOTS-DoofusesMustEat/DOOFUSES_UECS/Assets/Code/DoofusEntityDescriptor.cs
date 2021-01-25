using Svelto.ECS.EntityComponents;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, UECSEntityComponent, VelocityEntityComponent,
            SpeedEntityComponent, EGIDComponent, MealInfoComponent>
    {
    }

    public struct MealInfoComponent : IEntityComponent
    {
        public EGID targetMeal;
    }
}