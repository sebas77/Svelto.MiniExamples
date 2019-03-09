using System;
using System.Collections;
using Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class VelocityToPositionEngine : IQueryingEntitiesEngine
    {
        public void Ready() { ComputeVelocity().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator ComputeVelocity()
        {
            Action<VelocityEntityStruct[], int, IEntitiesDB> allEntitiesAction =
                (entities, count, entitiesDB) =>
                {
                    EGIDMapper<PositionEntityStruct> positionMapper =
                        entitiesDB.QueryMappedEntities<PositionEntityStruct>(entities[0].ID.groupID);

                    for (int i = 0; i < count; i++)
                    {
                        positionMapper.entity(entities[i].ID).position = new ECSVector3();
                    }
                };
            
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