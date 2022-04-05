using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    class FoodEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public FoodEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
        }
    }
}