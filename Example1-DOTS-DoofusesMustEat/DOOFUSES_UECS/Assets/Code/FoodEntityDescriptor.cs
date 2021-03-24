using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class FoodEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, SpawnPointEntityComponent, EGIDComponent, UECSEntityComponent>
    {
    }
}