using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class VelocityToPositionEngine : IQueryingEntitiesEngine
    {
        public void Ready() { ComputeVelocity().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator ComputeVelocity()
        {
            while (true)
            {
                var doofuses =
                    entitiesDB.QueryEntities<PositionEntityStruct, VelocityEntityStruct>(
                        GameGroups.DOOFUSES, out var count);

                for (int i = 0; i < count; i++)
                {
                    var ecsVector3 = doofuses.Item2[i].velocity;
                    
                    doofuses.Item1[i].position.Add(ecsVector3.Mul(Time.deltaTime));
                }

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { get; set; }
    }
}