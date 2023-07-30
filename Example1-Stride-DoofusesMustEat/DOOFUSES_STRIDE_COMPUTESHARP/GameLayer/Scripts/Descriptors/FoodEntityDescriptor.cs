using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.StrideExample.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    class FoodEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public FoodEntityDescriptor()
        {
            Add<MatrixComponent, PositionComponent>();
            Add<MealInfoComponent>();
        }
    }
}