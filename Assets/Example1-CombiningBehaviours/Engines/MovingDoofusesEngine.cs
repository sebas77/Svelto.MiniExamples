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
                var doofuses =
                    entitiesDB.QueryEntities<PositionEntityStruct>(
                        GameGroups.DOOFUSES, out var count);
                
                var foods = entitiesDB.QueryEntities<PositionEntityStruct>(
                                                                           GameGroups.FOOD, out var foodcount);

                for (int i = 0; i < count; i++)
                    for (int j = 0; j < foodcount; j++)
                {
                //    var direction = doofuses[i].position.
                }

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { get; set; }
    }
}