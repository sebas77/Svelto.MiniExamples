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
                    new ComponentBuilder<MatrixComponent>(),
                    new ComponentBuilder<PositionComponent>(),
                    new ComponentBuilder<RotationComponent>(),
                    new ComponentBuilder<VelocityComponent>(),
                    new ComponentBuilder<MealInfoComponent>(),
                    new ComponentBuilder<SpeedComponent>(),
                });
        }
    }
}