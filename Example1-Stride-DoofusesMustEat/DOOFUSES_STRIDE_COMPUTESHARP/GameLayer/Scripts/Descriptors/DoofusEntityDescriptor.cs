using Svelto.ECS.EntityComponents;
using Svelto.ECS.MiniExamples.Doofuses.StrideExample.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    class DoofusEntityDescriptor: ExtendibleEntityDescriptor<StrideEntityDescriptor>
    {
        public DoofusEntityDescriptor()
        {
            Add<StrideComponent>();
            ExtendWith(
                new IComponentBuilder[]
                {
                    new ComputeComponentBuilder<ComputeMatrixComponent>(),
                    new ComputeComponentBuilder<ComputePositionComponent>(),
                    new ComputeComponentBuilder<ComputeRotationComponent>(),
                    new ComputeComponentBuilder<ComputeVelocityComponent>(),
                    new ComponentBuilder<MealInfoComponent>(),
                    new ComputeComponentBuilder<ComputeSpeedComponent>(),
                });
        }
    }
}