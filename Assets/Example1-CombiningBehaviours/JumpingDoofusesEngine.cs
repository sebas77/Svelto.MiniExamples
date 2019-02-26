using System.Collections;
using Svelto.ECS.Components.Unity.Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class JumpingDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { JumpDoofuses().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator JumpDoofuses()
        {
            while (true)
            {
                var entities =
                    entitiesDB.QueryEntities<VelocityEntityStruct, InterpolateVector3EntityStruct>(
                        GameGroups.DOOFUSESJUMPING, out var count);

                for (int i = 0; i < count; i++)
                {
                    var time = entities.Item2[i].time;
                    if (time == 0)
                    {
                        entities.Item2[i].starPos = entities.Item1[i].velocity;
                        entities.Item2[i].endPos = entities.Item1[i].velocity;
                        entities.Item2[i].endPos.Add(0, 1, 0);
                    }
                    else
                        if (time < 1)
                        {
                        }
                        else
                        {
                            entities.Item2[i].time = 0;
                        
                            entities.Item2[i].starPos.Swap(ref entities.Item2[i].endPos);
                        }


                    entities.Item2[i].time += Time.deltaTime;
                }

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { get; set; }
    }
}