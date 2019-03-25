using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class VelocityToPositionDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { ComputeVelocity().RunOn(DoofusesStandardSchedulers.physicScheduler); }

        IEnumerator ComputeVelocity()
        {
            while (true)
            {
                var doofuses =
                    entitiesDB.QueryEntities<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct>(
                        GameGroups.DOOFUSES, out var count);

                for (int i = 0; i < count; i++)
                {
                    var ecsVector3 = doofuses.Item2[i].velocity;
                    
                    doofuses.Item1[i].position +=(ecsVector3 * (Time.deltaTime * doofuses.Item3[i].speed));
                }

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { get; set; }
    }

    struct SpeedEntityStruct : IEntityStruct
    {
        public float speed;
        public EGID ID
        {
            get;
            set;
        }
    }
}