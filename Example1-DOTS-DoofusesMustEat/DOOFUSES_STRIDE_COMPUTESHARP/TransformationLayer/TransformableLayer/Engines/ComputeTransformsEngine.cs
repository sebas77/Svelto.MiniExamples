using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.Common.Internal;

namespace  Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public enum TransformableLayerEngineNames
    {
        ComputeTransformsEngine
    }
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
                entitiesDB.FindGroups<PositionComponent, ScalingComponent, RotationComponent, MatrixComponent>();

            foreach (var ((position, scaling, rotation, transforms, count), _) in entitiesDB
               .QueryEntities<PositionComponent, ScalingComponent, RotationComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    Matrix.Transformation(ref scaling[i].scaling, ref rotation[i].rotation, ref position[i].position
                                        , out transforms[i].matrix);
                }
            }
        }
    }
}