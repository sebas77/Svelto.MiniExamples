using System.Runtime.CompilerServices;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.Common.Internal;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public enum TransformableLayerEngineNames
    {
        ComputeTransformsEngine
    }
    //todo I wanted to abstract too much here, Better to remove it and remove the matrix component
    /// <summary>
    /// Iterate all the entities that can be transformed and compute the transformation matrices out the transformation
    /// parameters
    /// </summary>
    [Sequenced(nameof(TransformableLayerEngineNames.ComputeTransformsEngine))]
    class ComputeTransformsEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var groups =
                entitiesDB.FindGroups<ComputePositionComponent, ComputeRotationComponent, ComputeMatrixComponent>();

            foreach (var ((position, rotation, transforms, count), _) in entitiesDB
               .QueryEntities<ComputePositionComponent, ComputeRotationComponent, ComputeMatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var scalingCenter = new Vector3(1, 1, 1);
                    ref var computePositionComponent = ref position[i];
                    var translation = new Vector3(computePositionComponent.position.X, computePositionComponent.position.Y, computePositionComponent.position.Z);
                    Matrix.Transformation(ref scalingCenter, ref rotation[i].rotation, ref translation, out Unsafe.As<System.Numerics.Matrix4x4, Matrix>(ref transforms[i].matrix));
                }
            }
        }
    }
}