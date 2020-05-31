using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine))]
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public void Ready() { }

        public LookingForFoodDoofusesEngine(IEntityFunctions nativeSwap)
        {
            _nativeDoofusesSwap = nativeSwap.ToNativeSwap<DoofusEntityDescriptor>();
            _nativeFoodSwap     = nativeSwap.ToNativeSwap<FoodEntityDescriptor>();
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
            var handle1 = CreateJobForDoofusesAndFood(_jobHandle
                                                    , GroupCompound<GameGroups.FOOD, GameGroups.RED,
                                                          GameGroups.NOTEATING>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED,
                                                          GameGroups.NOTEATING>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED,
                                                          GameGroups.EATING>.BuildGroup
                                                    , GroupCompound<GameGroups.FOOD, GameGroups.RED, GameGroups.EATING>
                                                         .BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            var handle2 = CreateJobForDoofusesAndFood(_jobHandle
                                                    , GroupCompound<GameGroups.FOOD, GameGroups.BLUE,
                                                          GameGroups.NOTEATING>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE,
                                                          GameGroups.NOTEATING>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE,
                                                          GameGroups.EATING>.BuildGroup
                                                    , GroupCompound<GameGroups.FOOD, GameGroups.BLUE, GameGroups.EATING>
                                                         .BuildGroup);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood
        (JobHandle inputDeps, FasterList<ExclusiveGroupStruct> foodGroups, FasterList<ExclusiveGroupStruct> doofusesGroups
       , ExclusiveGroupStruct swapDoofuseGroup, ExclusiveGroupStruct swapFoodGroup)
        {
            JobHandle deps = inputDeps;
            
            foreach (var foodgroup in entitiesDB.QueryEntities<EGIDComponent>(foodGroups).groups)
            {
                foreach (var doofusesGroup in entitiesDB.QueryEntities<MealInfoComponent, EGIDComponent>(doofusesGroups).groups)
                {
                    var doofusesCount = doofusesGroup.count;
                    var foodCount     = foodgroup.count;

                    if (foodCount == 0 || doofusesCount == 0)
                        return inputDeps;

                    var willEatDoofuses = math.min(foodCount, doofusesCount);

                    //schedule the job
                    deps = new LookingForFoodDoofusesJob()
                    {
                        _doofuses               = doofusesGroup.ToFast()
                      , _food                   = foodgroup.ToFast()
                      , _nativeDoofusesSwap     = _nativeDoofusesSwap
                      , _nativeFoodSwap         = _nativeFoodSwap
                      , _doofuseMealLockedGroup = swapDoofuseGroup
                      , _lockedFood             = swapFoodGroup
                    }.ScheduleParallel(willEatDoofuses, deps);
                }
            }

            return deps;
        }

        readonly NativeEntitySwap _nativeDoofusesSwap;
        readonly NativeEntitySwap _nativeFoodSwap;

        public EntitiesDB entitiesDB { private get; set; }

        [BurstCompile]
        struct LookingForFoodDoofusesJob : IJobParallelFor
        {
            public BT<NB<MealInfoComponent>, NB<EGIDComponent>> _doofuses;
            public BT<NB<EGIDComponent>>                        _food;
            public NativeEntitySwap                             _nativeDoofusesSwap;
            public NativeEntitySwap                             _nativeFoodSwap;
            public ExclusiveGroupStruct                         _doofuseMealLockedGroup;
            public ExclusiveGroupStruct                         _lockedFood;

#pragma warning disable 649
            [NativeSetThreadIndex] readonly int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                var targetMeal = _food.buffer[(uint) index].ID;
                _doofuses.buffer1[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                var @from = _doofuses.buffer2[index].ID;
                _nativeDoofusesSwap.SwapEntity(@from, _doofuseMealLockedGroup, _threadIndex);
                _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood, _threadIndex);
            }
        }
    }
}