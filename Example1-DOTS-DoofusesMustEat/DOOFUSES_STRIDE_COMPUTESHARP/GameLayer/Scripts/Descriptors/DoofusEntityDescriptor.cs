using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    class DoofusEntityDescriptor: ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public DoofusEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
            Add<StrideComponent>();
        }
    }
}