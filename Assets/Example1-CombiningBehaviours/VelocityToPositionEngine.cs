using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class VelocityToPositionEngine : IQueryingEntitiesEngine
    {
        public void Ready() { ComputeVelocity().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator ComputeVelocity()
        {
            AllEntitiesAction<VelocityEntityStruct, PositionEntityStruct> allEntitiesAction = ComputeVelocity;
            
            while (true)
            {
                entitiesDB.ExecuteOnAllEntities(allEntitiesAction);

                yield return null;
            }
        }

        static void ComputeVelocity(ref VelocityEntityStruct target, IEntitiesDB entitiesdb)
        {
            entitiesdb.QueryEntity<PositionEntityStruct>(target.ID);
        }

        public IEntitiesDB entitiesDB { get; set; }
    }
}