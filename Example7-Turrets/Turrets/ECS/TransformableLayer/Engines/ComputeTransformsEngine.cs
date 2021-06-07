using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ComputeTransformsEngine: SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        { }

        public override void Update()
        {
            LocalFasterReadOnlyList<ExclusiveGroupStruct> groups = entitiesDB.FindGroups<TRSComponent, MatrixComponent>();
            foreach (var ((trs, transforms, count), _) in entitiesDB.QueryEntities<TRSComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    Matrix.Transformation(ref trs[i].scaling, ref trs[i].rotation, ref trs[i].position, out transforms[i].matrix );
                }
            }
        }
    }
}