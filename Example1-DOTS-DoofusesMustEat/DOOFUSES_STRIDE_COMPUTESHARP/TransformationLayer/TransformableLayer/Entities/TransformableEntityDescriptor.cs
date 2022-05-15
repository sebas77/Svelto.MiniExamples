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
            //I wouldn't need to use explciitly ComputeComponentBuilder if it was mapped to the type
            //check 
            _components = new IComponentBuilder[]
            {
                new ComputeComponentBuilder<MatrixComponent>()
//              , new ComputeComponentBuilder<PositionComponent>()
//              , new ComputeComponentBuilder<ScalingComponent>()
//              , new ComputeComponentBuilder<RotationComponent>()
                
                // new ComponentBuilder<MatrixComponent>()
               , new ComponentBuilder<PositionComponent>()
               , new ComponentBuilder<ScalingComponent>()
               , new ComponentBuilder<RotationComponent>()
            };
        }
        
        static readonly IComponentBuilder[] _components;
   }
}