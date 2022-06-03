using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.Stride.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.Stride
{
    class FoodEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public FoodEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
        }
    }
}