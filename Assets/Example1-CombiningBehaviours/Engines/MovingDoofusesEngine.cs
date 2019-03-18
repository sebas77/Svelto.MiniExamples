using System.Collections;
using Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class MovingDoofusesEngine : IQueryingEntitiesEngine
    {
        public MovingDoofusesEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }
        
        public void Ready() { MoveDoofuses().RunOn(StandardSchedulers.coroutineScheduler); }

        IEnumerator MoveDoofuses()
        {
            while (true)
            {
                var doofuses =
                    entitiesDB.QueryEntities<PositionEntityStruct, VelocityEntityStruct>(GameGroups.DOOFUSESHUNGRY,
                                                                                         out var count);
                var foods = entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.FOOD, out var foodcount);

                for (int entityIndex = 0; entityIndex < count; entityIndex++)
                {
                    float currentMin = float.MaxValue;
                    ECSVector3 direction = new ECSVector3();

                    for (int j = 0; j < foodcount; j++)
                    {
                        var computeDirection = foods[j].position;
                        computeDirection.Sub(doofuses.Item1[entityIndex].position);
                        var sqrModule = computeDirection.SqrMagnitude();

                        if (currentMin > sqrModule)
                        {
                            currentMin = sqrModule;
                            direction = computeDirection;

                            if (sqrModule < 10)
                            {
                                _entityFunctions.SwapEntityGroup<DoofusEntityDescriptor>(doofuses.Item1[entityIndex].ID,
                                                                                         GameGroups.DOOFUSESEATING);
                                break; //close enough let's save some computations
                            }
                        }
                    }

                    doofuses.Item2[entityIndex].velocity.x = direction.x;
                    doofuses.Item2[entityIndex].velocity.z = direction.z;
                }

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { private get; set; }

        readonly IEntityFunctions _entityFunctions;
    }
}