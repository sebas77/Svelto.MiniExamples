using Stride.Engine;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ComputeHierarchicalTransformsEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            LocalFasterReadOnlyList<ExclusiveGroupStruct> groups =
                entitiesDB.FindGroups<ChildComponent, MatrixComponent>();
            foreach (var ((childComponent, transforms, count), _) in entitiesDB
               .QueryEntities<ChildComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    ref var parentMatrix = ref entitiesDB
                                              .QueryEntity<MatrixComponent>(
                                                   entitiesDB.GetEGID(childComponent[i].parent)).matrix;
                    ref var thisMatrix = ref transforms[i].matrix;
                    transforms[i].matrix = thisMatrix * parentMatrix;
                }
            }
        }
    }
}