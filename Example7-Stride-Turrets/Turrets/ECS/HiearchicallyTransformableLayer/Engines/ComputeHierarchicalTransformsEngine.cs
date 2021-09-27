using Stride.Engine;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ComputeHierarchicalTransformsEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var groups = entitiesDB.FindGroups<ChildComponent, MatrixComponent>();
            
            foreach (var ((childComponent, transforms, count), _) in entitiesDB
               .QueryEntities<ChildComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    ref var parentMatrix = ref entitiesDB.QueryEntity<MatrixComponent>(
                                                   entitiesDB.GetEGID(childComponent[i].parent)).matrix;
                    ref var thisMatrix = ref transforms[i].matrix;
                    transforms[i].matrix = thisMatrix * parentMatrix;
                }
            }
        }
    }
}