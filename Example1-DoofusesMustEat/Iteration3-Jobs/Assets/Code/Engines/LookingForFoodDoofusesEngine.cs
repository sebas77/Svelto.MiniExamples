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
            _nativeDoofusesSwap = nativeSwap.ToNativeSwap<DoofusEntityDescriptor>(nameof(LookingForFoodDoofusesEngine));
            _nativeFoodSwap     = nativeSwap.ToNativeSwap<FoodEntityDescriptor>(nameof(LookingForFoodDoofusesEngine));
        }
        
        public string name => nameof(LookingForFoodDoofusesEngine);

        public JobHandle Execute(JobHandle _jobHandle)
        {
            //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
            var handle1 = CreateJobForDoofusesAndFood(_jobHandle
                                                    , GameGroups.RED_FOOD_NOT_EATEN.Groups
                                                    , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
                                                    , GameGroups.RED_DOOFUSES_EATING.BuildGroup
                                                    , GameGroups.RED_FOOD_EATEN.BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            var handle2 = CreateJobForDoofusesAndFood(_jobHandle
                                                    , GameGroups.BLUE_FOOD_NOT_EATEN.Groups
                                                    , GameGroups.BLUE_DOOFUSES_NOT_EATING.Groups
                                                    , GameGroups.BLUE_DOOFUSES_EATING.BuildGroup
                                                    , GameGroups.BLUE_FOOD_EATEN.BuildGroup);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood
        (JobHandle inputDeps, FasterReadOnlyList<ExclusiveGroupStruct> foodGroups, FasterReadOnlyList<ExclusiveGroupStruct> doofusesGroups
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
                    deps = JobHandle.CombineDependencies(deps, new LookingForFoodDoofusesJob()
                    {
                        _doofuses               = doofusesGroup.ToBuffers()
                      , _food                   = foodgroup.ToBuffer()
                      , _nativeDoofusesSwap     = _nativeDoofusesSwap
                      , _nativeFoodSwap         = _nativeFoodSwap
                      , _doofuseMealLockedGroup = swapDoofuseGroup
                      , _lockedFood             = swapFoodGroup
                    }.ScheduleParallel(willEatDoofuses, inputDeps));
                }
            }

            return deps;
        }

        readonly NativeEntitySwap _nativeDoofusesSwap;
        readonly NativeEntitySwap _nativeFoodSwap;

        public EntitiesDB entitiesDB { private get; set; }

        [BurstCompile]
        struct LookingForFoodDoofusesJob : IJobParallelForBatch
        {
            public BT<NB<MealInfoComponent>, NB<EGIDComponent>> _doofuses;
            public BT<NB<EGIDComponent>>                        _food;
            public NativeEntitySwap                             _nativeDoofusesSwap;
            public NativeEntitySwap                             _nativeFoodSwap;
            public ExclusiveGroupStruct                         _doofuseMealLockedGroup;
            public ExclusiveGroupStruct                         _lockedFood;

#pragma warning disable 649
            /// <summary>
            /// _threadIndex will make the native entity operations thread safe
            /// </summary>
            [NativeSetThreadIndex] readonly int _threadIndex;
#pragma warning restore 649

            public void Execute(int startIndex, int count)
            {
                for (int index = startIndex; index < count; index++)
                {
                    //pickup the meal for this doofus
                    var targetMeal = _food.buffer[(uint) index].ID;
                    //Set the target meal for this doofus
                    _doofuses.buffer1[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                    //swap this doofus to the eating group so it won't be picked up again
                    _nativeDoofusesSwap.SwapEntity(@_doofuses.buffer2[index].ID, _doofuseMealLockedGroup, _threadIndex);
                    //swap the meal to the being eating group, so it won't be picked up again
                    _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood, _threadIndex);
                }
            }
        }
    }
}