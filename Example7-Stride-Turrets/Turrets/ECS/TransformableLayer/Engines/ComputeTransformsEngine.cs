using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ComputeTransformsEngine : IQueryingEntitiesEngine, IUpdateEngine
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