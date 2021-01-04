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

        public JobHandle Execute(JobHandle inputDeps)
        {
            //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
            var handle1 = CreateJobForDoofusesAndFood(inputDeps, GameGroups.RED_FOOD_NOT_EATEN.Groups
                                                    , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
                                                    , GameGroups.RED_DOOFUSES_EATING.BuildGroup
                                                    , GameGroups.RED_FOOD_EATEN.BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            var handle2 = CreateJobForDoofusesAndFood(inputDeps, GameGroups.BLUE_FOOD_NOT_EATEN.Groups
                                                    , GameGroups.BLUE_DOOFUSES_NOT_EATING.Groups
                                                    , GameGroups.BLUE_DOOFUSES_EATING.BuildGroup
                                                    , GameGroups.BLUE_FOOD_EATEN.BuildGroup);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        /// <summary>
        /// All the available doofuses will start to hunt for available food
        /// </summary>
        JobHandle CreateJobForDoofusesAndFood
        (JobHandle inputDeps, FasterReadOnlyList<ExclusiveGroupStruct> availableFood
       , FasterReadOnlyList<ExclusiveGroupStruct> availableDoofuses, ExclusiveBuildGroup eatingDoofusesGroup
       , ExclusiveBuildGroup eatenFoodGroup)
        {
            JobHandle combinedDeps = inputDeps;

            foreach (var ((foodEntities, availableFoodCount), _) in entitiesDB.QueryEntities<EGIDComponent>(
                availableFood))
            {
                foreach (var ((doofuses, egids, doofusesCount), _) in entitiesDB
                   .QueryEntities<MealInfoComponent, EGIDComponent>(availableDoofuses))
                {
                    var willEatDoofuses = math.min(availableFoodCount, doofusesCount);

                    //schedule the job
                    combinedDeps = JobHandle.CombineDependencies(combinedDeps, new LookingForFoodDoofusesJob()
                    {
                        _doofuses               = doofuses
                      , _doofusesegids          = egids
                      , _food                   = foodEntities
                      , _nativeDoofusesSwap     = _nativeDoofusesSwap
                      , _nativeFoodSwap         = _nativeFoodSwap
                      , _doofuseMealLockedGroup = eatingDoofusesGroup
                      , _lockedFood             = eatenFoodGroup
                    }.ScheduleParallel(willEatDoofuses, inputDeps));
                }
            }

            return combinedDeps;
        }

        readonly NativeEntitySwap _nativeDoofusesSwap;
        readonly NativeEntitySwap _nativeFoodSwap;

        public EntitiesDB entitiesDB { private get; set; }

        [BurstCompile]
        struct LookingForFoodDoofusesJob : IJobParallelFor
        {
            public NB<MealInfoComponent> _doofuses;
            public NB<EGIDComponent>     _food;
            public NativeEntitySwap      _nativeDoofusesSwap;
            public NativeEntitySwap      _nativeFoodSwap;
            public ExclusiveBuildGroup   _doofuseMealLockedGroup;
            public ExclusiveBuildGroup   _lockedFood;
            public NB<EGIDComponent>     _doofusesegids;

#pragma warning disable 649
            /// <summary>
            /// _threadIndex will make the native entity operations thread safe
            /// </summary>
            [NativeSetThreadIndex] readonly int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                //pickup the meal for this doofus
                var targetMeal = _food[(uint) index].ID;
                //Set the target meal for this doofus
                _doofuses[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                //swap this doofus to the eating group so it won't be picked up again
                _nativeDoofusesSwap.SwapEntity(@_doofusesegids[index].ID, _doofuseMealLockedGroup, _threadIndex);
                //swap the meal to the being eating group, so it won't be picked up again
                _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood, _threadIndex);
            }
        }
    }
}