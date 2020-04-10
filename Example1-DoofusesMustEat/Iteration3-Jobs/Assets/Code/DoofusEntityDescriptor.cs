using Svelto.ECS.EntityComponents;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, UnityEcsEntityComponent, VelocityEntityComponent,
            SpeedEntityComponent, EGIDComponent, MealInfoComponent>
    {
    }

    public struct MealInfoComponent : IEntityComponent
    {
        public EGID targetMeal;
    }
}