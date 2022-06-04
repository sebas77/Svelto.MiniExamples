using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Internal;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
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
            var handle1 = CreateJobForDoofusesAndFood(inputDeps
                                                    , GameGroups.RED_FOOD_NOT_EATEN.Groups
                                                    , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
                                                    , GameGroups.RED_DOOFUSES_EATING.BuildGroup
                                                    , GameGroups.RED_FOOD_EATEN.BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            var handle2 = CreateJobForDoofusesAndFood(inputDeps
                                                    , GameGroups.BLUE_FOOD_NOT_EATEN.Groups
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
        (JobHandle inputDeps, FasterReadOnlyList<ExclusiveGroupStruct> groupsWithAvailableFood
       , FasterReadOnlyList<ExclusiveGroupStruct> groupsWithAvailableDoofuses, ExclusiveBuildGroup eatingDoofusesGroup
       , ExclusiveBuildGroup eatenFoodGroup)
        {
            JobHandle combinedDeps = inputDeps;

            //query all the available food
            var availableFoodComponents = entitiesDB.QueryEntities<PositionEntityComponent>(groupsWithAvailableFood).GetEnumerator();
            //query all the doofuses that are not eating
            var availableDoofusesComponents = entitiesDB.QueryEntities<MealInfoComponent>(groupsWithAvailableDoofuses).GetEnumerator();

            while (availableFoodComponents.MoveNext() && availableDoofusesComponents.MoveNext())
            {
                var ((_, foodIDs, availableFoodCount), currentFoodGroup) = availableFoodComponents.Current;
                var ((doofusesEntities, doofusesIDs, doofusesCount), currentDoofusesGroup) = availableDoofusesComponents.Current;
                {
                    var willEatDoofuses = math.min(availableFoodCount, doofusesCount);

                    if (willEatDoofuses > 0)
                    {
                        //schedule the job
                        combinedDeps = JobHandle.CombineDependencies(combinedDeps, new LookingForFoodDoofusesJob()
                        {
                            _doofuses               = doofusesEntities
                          , _doofusesids            = doofusesIDs
                          , _food                   = foodIDs
                          , _nativeDoofusesSwap     = _nativeDoofusesSwap
                          , _nativeFoodSwap         = _nativeFoodSwap
                          , _doofuseMealLockedGroup = eatingDoofusesGroup
                          , _lockedFood             = eatenFoodGroup
                          , _fromFoodGroup          = currentFoodGroup
                          , _fromDoofusesGroup      = currentDoofusesGroup
                        }.ScheduleParallel(willEatDoofuses, inputDeps));
                    }
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
            public NativeEntityIDs       _food;
            public NativeEntitySwap      _nativeDoofusesSwap;
            public NativeEntitySwap      _nativeFoodSwap;
            public ExclusiveBuildGroup   _doofuseMealLockedGroup;
            public ExclusiveBuildGroup   _lockedFood;
            public NativeEntityIDs       _doofusesids;
            public ExclusiveGroupStruct  _fromFoodGroup;
            public ExclusiveGroupStruct  _fromDoofusesGroup;

#pragma warning disable 649
            /// <summary>
            /// _threadIndex will make the native entity operations thread safe
            /// </summary>
            [NativeSetThreadIndex] readonly int _threadIndex;
#pragma warning restore 649

            public void Execute(int index)
            {
                //pickup the meal for this doofus
                var targetMeal = new EGID(_food[(uint)index], _fromFoodGroup);
                //Set the target meal for this doofus
                _doofuses[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                //swap this doofus to the eating group so it won't be picked up again
                _nativeDoofusesSwap.SwapEntity(new EGID(_doofusesids[index], _fromDoofusesGroup)
                                             , _doofuseMealLockedGroup, _threadIndex);
                //swap the meal to the being eating group, so it won't be picked up again
                _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood, _threadIndex);
            }
        }
    }
}