using System.Collections;
using Svelto.ECS.Components.Unity.Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class MovingDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { MoveDoofuses().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator MoveDoofuses()
        {
            while (true)
            {
                var entities =
                    entitiesDB.QueryEntities<PositionEntityStruct, InterpolateVector3EntityStruct>(
                        GameGroups.DOOFUSES, out var count);

                for (int i = 0; i < count; i++)
                {
                    var time = entities.Item2[i].time;
                    if (time == 0)
                    {
                        entities.Item2[i].starPos = entities.Item1[i].position;
                        entities.Item2[i].endPos = entities.Item1[i].position;
                        entities.Item2[i].endPos.Add(0, 0, 1);
                    }
                    else
                    if (time < 1)
                    {
                        entities.Item1[i].position.Interpolate(ref entities.Item2[i].starPos,
                                                               ref entities.Item2[i].endPos, 
                                                               time);
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