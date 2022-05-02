using Svelto.ECS.ComputeSharp;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public class TransformableEntityDescriptor : IEntityDescriptor 
    {
        public IComponentBuilder[] componentsToBuild { get; }

        public TransformableEntityDescriptor()
        {
            componentsToBuild = _components;
        }

        static TransformableEntityDescriptor()
        {
            _components = new IComponentBuilder[]
            {
                new ComputeComponentBuilder<MatrixComponent>()
              , new ComputeComponentBuilder<PositionComponent>()
              , new ComputeComponentBuilder<ScalingComponent>()
              , new ComputeComponentBuilder<RotationComponent>()
            };
        }
        
        static readonly IComponentBuilder[] _components;
   }
}