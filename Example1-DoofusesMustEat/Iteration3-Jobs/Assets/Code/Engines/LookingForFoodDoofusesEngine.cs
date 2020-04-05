using System;
using System.Threading;
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

        public EntitiesDB entitiesDB { private get; set; }

        JobHandle CreateJobForDoofusesAndFood
            (JobHandle inputDeps, ExclusiveGroup[] foodGroups, ExclusiveGroup[] doofusesGroups)
        {
            var foodEntityGroups =
                entitiesDB.NativeGroupsIterator<PositionEntityComponent, MealEntityComponent>(foodGroups);
            var doofusesEntityGroups =
                entitiesDB.NativeGroupsIterator<PositionEntityComponent, VelocityEntityComponent, EGIDComponent>(
                    doofusesGroups);

            JobHandle combinedDependencies = default;

            foreach (var foodBuffer in foodEntityGroups)
            {
                foreach (var doofusesBuffer in doofusesEntityGroups)
                {
                    //schedule the job
                    var deps = new LookingForFoodDoofusesJob(doofusesBuffer, foodBuffer).Schedule(
                        (int) doofusesBuffer.count, ProcessorCount.Batch(doofusesBuffer.count), inputDeps);

                    //Never forget to dispose the buffer (may change this in future)
                    combinedDependencies = doofusesBuffer.CombineDispose(combinedDependencies, deps);
                }

                //Never forget to dispose the buffer (may change this in future)
                combinedDependencies = foodBuffer.CombineDispose(combinedDependencies, combinedDependencies);
            }

            return combinedDependencies;
        }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            var handle1 = CreateJobForDoofusesAndFood(_jobHandle, GroupCompound<GameGroups.FOOD, GameGroups.RED>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.RED,
                                                          GameGroups.NOTEATING>.Groups);
            var handle2 = CreateJobForDoofusesAndFood(_jobHandle, GroupCompound<GameGroups.FOOD, GameGroups.BLUE>.Groups
                                                    , GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE,
                                                          GameGroups.NOTEATING>.Groups);

            return JobHandle.CombineDependencies(handle1, handle2);
        }
    }

    [BurstCompile]
    public struct LookingForFoodDoofusesJob : IJobParallelFor
    {
        readonly BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<VelocityEntityComponent>,
            NativeBuffer<EGIDComponent>> _doofuses;

        readonly BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<MealEntityComponent>> _food;

        readonly NativeEntityRemove
            _nativeRemove;

        public LookingForFoodDoofusesJob
        (in BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<VelocityEntityComponent>,
             NativeBuffer<EGIDComponent>> doofuses
       , in BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<MealEntityComponent>> food) : this()
        {
            _doofuses         = doofuses;
            _food             = food;
        }

        public void Execute(int index)
        {
            float currentMin = float.MaxValue;

            ref var velocityEntityComponent = ref _doofuses.buffer2[index];
            ref var positionEntityComponent = ref _doofuses.buffer1[index];

            var foodcountLength = _food.count;
            var foodPositions   = _food.buffer1;
            var mealInfos       = _food.buffer2;

            float3 closerComputeDirection = default;
            for (int foodIndex = 0; foodIndex < foodcountLength; ++foodIndex)
            {
                var computeDirection = foodPositions[foodIndex].position - positionEntityComponent.position;

                var sqrModule = computeDirection.x * computeDirection.x + computeDirection.y * computeDirection.y
                                                                        + computeDirection.z * computeDirection.z;

                if (currentMin > sqrModule)
                {
                    currentMin             = sqrModule;
                    closerComputeDirection = computeDirection;

                    //food found
                    if (sqrModule < 2)
                    {
                        Interlocked.Increment(ref mealInfos[foodIndex].eaters);

                        closerComputeDirection = default;

                        break; //close enough let's save some computations
                    }
                }
            }

            //going toward food, not breaking as closer food can spawn
            velocityEntityComponent.velocity.x = closerComputeDirection.x;
            velocityEntityComponent.velocity.z = closerComputeDirection.z;
        }
    }
}