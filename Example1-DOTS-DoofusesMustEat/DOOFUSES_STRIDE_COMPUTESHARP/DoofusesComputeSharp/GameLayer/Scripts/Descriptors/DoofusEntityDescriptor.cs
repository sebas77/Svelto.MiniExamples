using Svelto.ECS.EntityComponents;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    class DoofusEntityDescriptor: ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public DoofusEntityDescriptor()
        {
            Add<VelocityEntityComponent, SpeedEntityComponent, MealInfoComponent>();
        }
    }
}