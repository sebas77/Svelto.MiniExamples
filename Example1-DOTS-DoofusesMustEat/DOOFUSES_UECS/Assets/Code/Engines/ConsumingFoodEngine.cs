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
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public void Ready() { }

        public ConsumingFoodEngine(IEntityFunctions nativeOptions)
        {
            _nativeSwap   = nativeOptions.ToNativeSwap<DoofusEntityDescriptor>(nameof(ConsumingFoodEngine));
            _nativeRemove = nativeOptions.ToNativeRemove<FoodEntityDescriptor>(nameof(ConsumingFoodEngine));
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            //Iterate EATING RED doofuses to move toward locked food and move to NOEATING if food is ATE
            //todo: this is a double responsibility. Move toward food and eating the food may work in separate engines
            var handle1 = CreateJobForDoofusesAndFood(_jobHandle, GameGroups.RED_DOOFUSES_EATING.Groups
                                                    , GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
                                                    , GameGroups.RED_FOOD_EATEN.Groups);
            //Iterate EATING BLUE doofuses to look for BLUE food and MOVE them to NOEATING if food is ATE
            //todo: this is a double responsibility. Move toward food and eating the food may work in separate engines
            var handle2 = CreateJobForDoofusesAndFood(_jobHandle, GameGroups.BLUE_DOOFUSES_EATING.Groups
                                                    , GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
                                                    , GameGroups.BLUE_FOOD_EATEN.Groups);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        public string name => nameof(ConsumingFoodEngine);

        JobHandle CreateJobForDoofusesAndFood(JobHandle inputDeps, in LocalFasterReadOnlyList<ExclusiveGroupStruct> 
                doofusesEatingGroups, ExclusiveBuildGroup foodEatenGroup, in LocalFasterReadOnlyList<ExclusiveGroupStruct> foodGroup)
        {
            var foodPositionMapper = entitiesDB.QueryNativeMappedEntities<PositionEntityComponent>(foodGroup);
            
            //against all the doofuses
            JobHandle deps = inputDeps;
            foreach (var (doofusesBuffer, _) in entitiesDB
               .QueryEntities<PositionEntityComponent, VelocityEntityComponent, MealInfoComponent, EGIDComponent>(
                    doofusesEatingGroups))
            {
                var doofuses      = doofusesBuffer.ToBuffers();
                var doofusesCount = doofuses.count;

                //schedule the job
                deps = JobHandle.CombineDependencies(
                    deps, new ConsumingFoodJob(doofuses, foodPositionMapper, _nativeSwap, _nativeRemove, foodEatenGroup)
                       .ScheduleParallel(doofusesCount, inputDeps));
            }

            foodPositionMapper.ScheduleDispose(deps);

            return deps;
        }

        readonly NativeEntitySwap   _nativeSwap;
        readonly NativeEntityRemove _nativeRemove;

        public EntitiesDB entitiesDB { private get; set; }
    }

    [BurstCompile]
    public readonly struct ConsumingFoodJob : IJobParallelFor
    {
        readonly BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>, NB<EGIDComponent>>
            _doofuses;

        readonly NativeEGIDMultiMapper<PositionEntityComponent> _foodPositionMapper;
        readonly NativeEntitySwap                               _nativeSwap;
        readonly NativeEntityRemove                             _nativeRemove;

        [NativeSetThreadIndex] readonly int                 _threadIndex;
        readonly                        ExclusiveBuildGroup _doofuseMealLockedGroup;

        public ConsumingFoodJob
        (in BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>, NB<EGIDComponent>>
             doofuses, NativeEGIDMultiMapper<PositionEntityComponent> foodPositionMapper, NativeEntitySwap swap
       , NativeEntityRemove nativeRemove, ExclusiveBuildGroup doofuseMealLockedGroup) : this()
        {
            _doofuses               = doofuses;
            _foodPositionMapper     = foodPositionMapper;
            _nativeSwap             = swap;
            _nativeRemove           = nativeRemove;
            _doofuseMealLockedGroup = doofuseMealLockedGroup;
            _threadIndex            = 0;
        }

        public void Execute(int index)
        {
            ref EGID   mealInfoEGID = ref _doofuses.buffer3[index].targetMeal;
            ref float3 doofusPosition    = ref _doofuses.buffer1[index].position;
            ref float3 velocity          = ref _doofuses.buffer2[index].velocity;
            ref float3 foodPosition      = ref _foodPositionMapper.Entity(mealInfoEGID).position;

            var computeDirection = foodPosition - doofusPosition;
            var sqrModule        = computeDirection.x * computeDirection.x + computeDirection.z * computeDirection.z;

            //close enough to the food
            if (sqrModule < 2)
            {
                velocity.x = 0;
                velocity.z = 0;

                //food found
                //Change Doofuses State
                _nativeSwap.SwapEntity(_doofuses.buffer4[index].ID, _doofuseMealLockedGroup, _threadIndex);
                //Remove Eaten Food
                _nativeRemove.RemoveEntity(mealInfoEGID, _threadIndex);

                return;
            }

            //going toward food, not breaking as closer food can spawn
            velocity.x = computeDirection.x;
            velocity.z = computeDirection.z;
        }
    }
}