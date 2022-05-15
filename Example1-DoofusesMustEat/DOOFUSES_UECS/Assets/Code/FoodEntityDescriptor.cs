using Svelto.ECS.EntityComponents;
using Svelto.ECS.SveltoOnDOTS;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class FoodEntityDescriptor : GenericEntityDescriptor<PositionEntityComponent, SpawnPointEntityComponent,
        DOTSEntityComponent>
    {
    }
}