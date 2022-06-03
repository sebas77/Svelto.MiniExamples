using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.Stride.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.Stride
{
    class DoofusEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public DoofusEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
        }
    }
}