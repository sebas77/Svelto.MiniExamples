using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    public class LookingForFoodDoofusesEngine : JobComponentSystem, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public IEntitiesDB entitiesDB { private get; set; }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle1 = CreateJobForDoofusesAndFood(inputDeps, GroupCompound<GameGroups.FOOD, GameGroups.RED>.Groups,
                                                      GroupCompound<GameGroups.DOOFUSES, GameGroups.RED>.Groups);
            var handle2 = CreateJobForDoofusesAndFood(inputDeps, GroupCompound<GameGroups.FOOD, GameGroups.BLUE>.Groups,
                                                      GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE>.Groups);

            return JobHandle.CombineDependencies(handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood(JobHandle inputDeps, ExclusiveGroup[] foodGroups,
                                              ExclusiveGroup[] doofusesGroups)
        {
            var foodEntityGroups = entitiesDB.GroupIterators<PositionEntityStruct, MealEntityStruct>(foodGroups);
            var doofusesEntityGroups =
                entitiesDB
                   .GroupIterators<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(doofusesGroups);
            
            JobHandle combinedDependencies = default;

            foreach (var foodBuffer in foodEntityGroups)
            {
                foreach (var doofusesBuffer in doofusesEntityGroups)
                {
                    //schedule the job
                    var deps =
                        new LookingForFoodDoofusesJob(doofusesBuffer, foodBuffer).Schedule((int) doofusesBuffer.count,
                                                                                           (int) (doofusesBuffer.count /
                                                                                                  8), inputDeps);

                    //Never forget to dispose the buffer (may change this in future)
                    combinedDependencies = doofusesBuffer.CombineDispose(combinedDependencies, deps);
                }

                //Never forget to dispose the buffer (may change this in future)
                combinedDependencies = foodBuffer.CombineDispose(combinedDependencies, combinedDependencies);
            }

            return combinedDependencies;
        }
    }

    [BurstCompile]
    public struct LookingForFoodDoofusesJob : IJobParallelFor
    {
        readonly BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
            NativeBuffer<HungerEntityStruct>> _doofuses;

        readonly BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<MealEntityStruct>> _foodcount;

        public LookingForFoodDoofusesJob(
            in BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                NativeBuffer<HungerEntityStruct>> doofuses,
            in BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<MealEntityStruct>> foodcount)
        {
            _doofuses  = doofuses;
            _foodcount = foodcount;
        }

        public void Execute(int index)
        {
            float currentMin = float.MaxValue;

            ref var velocityEntityStruct = ref _doofuses.buffer2[index];
            ref var hungerEntityStruct   = ref _doofuses.buffer3[index];
            ref var positionEntityStruct = ref _doofuses.buffer1[index];

            var foodcountLength = _foodcount.count;

            float3 closerComputeDirection = default;
            for (int foodIndex = 0; foodIndex < foodcountLength; ++foodIndex)
            {
                var computeDirection = _foodcount.buffer1[foodIndex].position - positionEntityStruct.position;

                var sqrModule = computeDirection.x * computeDirection.x + computeDirection.y * computeDirection.y +
                                computeDirection.z * computeDirection.z;

                if (currentMin > sqrModule)
                {
                    currentMin             = sqrModule;
                    closerComputeDirection = computeDirection;

                    //food found
                    if (sqrModule < 2)
                    {
                        hungerEntityStruct.hunger -= 2;
                        Interlocked.Increment(ref _foodcount.buffer2[foodIndex].eaters);

                        closerComputeDirection = default;

                        break; //close enough let's save some computations
                    }
                }
            }

            //going toward food, not breaking as closer food can spawn
            velocityEntityStruct.velocity.x = closerComputeDirection.x;
            velocityEntityStruct.velocity.z = closerComputeDirection.z;

            hungerEntityStruct.hunger++;
        }
    }
}