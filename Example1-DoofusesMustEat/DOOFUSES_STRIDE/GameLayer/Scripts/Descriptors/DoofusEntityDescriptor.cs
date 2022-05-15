using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    class DoofusEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public DoofusEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
        }
    }
}