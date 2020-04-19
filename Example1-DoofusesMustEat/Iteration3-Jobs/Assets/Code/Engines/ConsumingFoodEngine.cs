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
            _nativeSwap    = nativeOptions.ToNativeSwap<DoofusEntityDescriptor>();
            _nativeRemove  = nativeOptions.ToNativeRemove<FoodEntityDescriptor>();
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            //Iterate EATING RED doofuses to move toward locked food and move to NOEATING if food is ATE
            //todo: this is a double responsibility. Move toward food and eating the food may work in separate engines
            var handle1 = CreateJobForDoofusesAndFood(
                _jobHandle, GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.EATING>.Groups
              , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.NOTEATING>.BuildGroup
              , GroupCompound<GameGroups.FOOD, GameGroups.RED, GameGroups.EATING>.BuildGroup);
            //Iterate EATING BLUE doofuses to look for BLUE food and MOVE them to NOEATING if food is ATE
            //todo: this is a double responsibility. Move toward food and eating the food may work in separate engines
            var handle2 = CreateJobForDoofusesAndFood(
                _jobHandle, GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.EATING>.Groups
              , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.NOTEATING>.BuildGroup
              , GroupCompound<GameGroups.FOOD, GameGroups.BLUE, GameGroups.EATING>.BuildGroup);

            //can run in parallel
            return JobHandle.CombineDependencies(handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood
        (JobHandle inputDeps, ExclusiveGroupStruct[] doofusesGroups, ExclusiveGroupStruct swapGroup
       , ExclusiveGroupStruct foodGroup)
        {
            if (entitiesDB.TryQueryNativeMappedEntities<PositionEntityComponent>(foodGroup, out var foodPositionMapper)
             == false) return inputDeps;

            var doofusesEntityGroups = entitiesDB
                   .NativeGroupsIterator<PositionEntityComponent, VelocityEntityComponent, MealInfoComponent,
                        EGIDComponent>(doofusesGroups);

            //against all the doofuses
            foreach (var doofusesBuffer in doofusesEntityGroups)
            {
                var doofusesCount = doofusesBuffer.count;

                //schedule the job
                var deps = new ConsumingFoodJob(doofusesBuffer, foodPositionMapper, _nativeSwap, _nativeRemove, swapGroup)
                       .ScheduleParallel(doofusesCount, inputDeps);
                
                //Never forget to dispose the buffer (may change this in future)
                doofusesBuffer.ScheduleDispose(deps);
                
                inputDeps = JobHandle.CombineDependencies(deps, inputDeps);
            }

            return inputDeps;
        }

        readonly NativeEntitySwap _nativeSwap;
        readonly NativeEntityRemove _nativeRemove;

        public EntitiesDB entitiesDB { private get; set; }
    }

    [BurstCompile]
    public readonly struct ConsumingFoodJob : IJobParallelFor
    {
        readonly BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>, NB<EGIDComponent>>
            _doofuses;

        readonly NativeEGIDMapper<PositionEntityComponent> _foodPosition;
        readonly NativeEntitySwap                          _nativeSwap;
        readonly NativeEntityRemove                        _nativeRemove;

        [NativeSetThreadIndex] readonly int                  _threadIndex;
        readonly                        ExclusiveGroupStruct _doofuseMealLockedGroup;

        public ConsumingFoodJob
        (in BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>, NB<EGIDComponent>>
             doofuses, NativeEGIDMapper<PositionEntityComponent> foodPosition, NativeEntitySwap swap
       , NativeEntityRemove nativeRemove, ExclusiveGroupStruct doofuseMealLockedGroup) : this()
        {
            _doofuses               = doofuses;
            _foodPosition           = foodPosition;
            _nativeSwap             = swap;
            _nativeRemove           = nativeRemove;
            _doofuseMealLockedGroup = doofuseMealLockedGroup;
            _threadIndex            = 0;
        }

        public void Execute(int index)
        {
            ref var    mealInfoComponent = ref _doofuses.buffer3[index];
            EGID       lockedFood        = mealInfoComponent.targetMeal;
            ref float3 foodPosition      = ref _foodPosition.Entity(lockedFood.entityID).position;
            ref float3 doofusPosition    = ref _doofuses.buffer1[index].position;
            ref var    velocity          = ref _doofuses.buffer2[index].velocity;
            
            var computeDirection = foodPosition - doofusPosition;
            var sqrModule = computeDirection.x * computeDirection.x + computeDirection.z * computeDirection.z;

            if (sqrModule < 2)
            {
                velocity.x = 0;
                velocity.z = 0;
                
                //food found
                //Change Doofuses State
                _nativeSwap.SwapEntity(_doofuses.buffer4[index].ID, _doofuseMealLockedGroup, _threadIndex);
                //Remove Eaten Food
                _nativeRemove.RemoveEntity(lockedFood, _threadIndex);

                return;
            }

            //going toward food, not breaking as closer food can spawn
            velocity.x = computeDirection.x;
            velocity.z = computeDirection.z;
        }
    }
}