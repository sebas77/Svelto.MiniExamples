using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.GameObjectsLayer;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, GameObjectEntityComponent, VelocityEntityComponent,
            SpeedEntityComponent, EGIDComponent, MealInfoComponent>
    {
    }
}