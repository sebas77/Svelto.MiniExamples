using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine))]
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public void Ready() { }

        public LookingForFoodDoofusesEngine(IEntityFunctions nativeSwap)
        {
            _nativeDoofusesSwap = nativeSwap.ToNativeSwap<DoofusEntityDescriptor>();
            _nativeFoodSwap = nativeSwap.ToNativeSwap<FoodEntityDescriptor>();
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
            var handle1 = CreateJobForDoofusesAndFood(_jobHandle
                 , GroupCompound<GameGroups.FOOD, GameGroups.RED, GameGroups.NOTEATING>.Groups
                 , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.NOTEATING>.Groups
                                                      
                 , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.EATING>.BuildGroup
                 , GroupCompound<GameGroups.FOOD, GameGroups.RED, GameGroups.EATING>.BuildGroup);
            
            
            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            var handle2 = CreateJobForDoofusesAndFood(_jobHandle
                , GroupCompound<GameGroups.FOOD, GameGroups.BLUE, GameGroups.NOTEATING>.Groups
                , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.NOTEATING>.Groups
                                                      
                , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.EATING>.BuildGroup
                , GroupCompound<GameGroups.FOOD, GameGroups.BLUE, GameGroups.EATING>.BuildGroup);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood(JobHandle inputDeps, ExclusiveGroup[] foodGroups, ExclusiveGroup[] doofusesGroups
                                            , ExclusiveGroupStruct swapDoofuseGroup, ExclusiveGroupStruct swapFoodGroup)
        {
            var foodBuffer = entitiesDB.NativeEntitiesBuffer<EGIDComponent, PositionEntityComponent>(foodGroups[0]);
            var doofusesBuffer = entitiesDB.NativeEntitiesBuffer<MealInfoComponent, EGIDComponent>(doofusesGroups[0]);

            var doofusesCount = doofusesBuffer.count;
            var foodCount = foodBuffer.count;

            if (foodCount == 0 || doofusesCount == 0)
                return inputDeps;
            
            JobHandle innerCombinedDependencies = default;

            var willEatDoofuses = math.min(foodCount, doofusesCount);

            //schedule the job
            var deps = new LookingForFoodDoofusesJob()
            {
                _doofuses = doofusesBuffer, _food = foodBuffer, _nativeDoofusesSwap = _nativeDoofusesSwap,
                _nativeFoodSwap  = _nativeFoodSwap, _doofuseMealLockedGroup = swapDoofuseGroup,
                _lockedFood = swapFoodGroup
            }.ScheduleParallel(willEatDoofuses, inputDeps);

            //Never forget to dispose the buffer (may change this in future)
            innerCombinedDependencies = doofusesBuffer.CombineDispose(foodBuffer, innerCombinedDependencies, deps);

            return innerCombinedDependencies;
        }

        readonly NativeEntitySwap _nativeDoofusesSwap;
        readonly NativeEntitySwap _nativeFoodSwap;

        public EntitiesDB entitiesDB { private get; set; }

        [BurstCompile]
        struct LookingForFoodDoofusesJob : IJobParallelFor
        {
            public BT<NB<MealInfoComponent>, NB<EGIDComponent>>       _doofuses;
            public BT<NB<EGIDComponent>, NB<PositionEntityComponent>> _food;
            public NativeEntitySwap                                   _nativeDoofusesSwap;
            public NativeEntitySwap                                   _nativeFoodSwap;
            public ExclusiveGroupStruct                               _doofuseMealLockedGroup;
            public ExclusiveGroupStruct                               _lockedFood;

            [NativeSetThreadIndex] readonly int                  _threadIndex;

            public void Execute(int index)
            {
                var targetMeal = _food.buffer1[(uint) index].ID;
               _doofuses.buffer1[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                _nativeDoofusesSwap.SwapEntity(_doofuses.buffer2[index].ID, _doofuseMealLockedGroup, _threadIndex);
                _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood, _threadIndex);
            }

            public void Execute(int startIndex, int count)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}