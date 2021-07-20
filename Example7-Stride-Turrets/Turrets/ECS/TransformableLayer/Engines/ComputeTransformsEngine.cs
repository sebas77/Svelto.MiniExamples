using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ComputeTransformsEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
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