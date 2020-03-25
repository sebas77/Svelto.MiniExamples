using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class LookingForFoodDoofusesEngine : SystemBase, IQueryingEntitiesEngine
    {
        public void Ready()
        {
        }

        public EntitiesDB entitiesDB { private get; set; }
        
        protected override void OnUpdate()
        {
            var handle1 = CreateJobForDoofusesAndFood(Dependency, GroupCompound<GameGroups.FOOD, GameGroups.RED>.Groups,
                GroupCompound<GameGroups.DOOFUSES, GameGroups.RED, GameGroups.NOTEATING>.Groups);
            var handle2 = CreateJobForDoofusesAndFood(Dependency, GroupCompound<GameGroups.FOOD, GameGroups.BLUE>.Groups,
                GroupCompound<GameGroups.DOOFUSES, GameGroups.BLUE, GameGroups.NOTEATING>.Groups);
            
            this.Dependency = JobHandle.CombineDependencies(Dependency, handle1, handle2);
        }

        JobHandle CreateJobForDoofusesAndFood(JobHandle inputDeps, ExclusiveGroup[] foodGroups,
            ExclusiveGroup[] doofusesGroups)
        {
            var foodEntityGroups = entitiesDB.NativeGroupsIterator<PositionEntityStruct, MealEntityStruct>(foodGroups);
            var doofusesEntityGroups =
                entitiesDB.NativeGroupsIterator<PositionEntityStruct, VelocityEntityStruct>(doofusesGroups);

            JobHandle combinedDependencies = default;

            foreach (var foodBuffer in foodEntityGroups)
            {
                foreach (var doofusesBuffer in doofusesEntityGroups)
                {
                    //schedule the job
                    var deps =
                        new LookingForFoodDoofusesJob(doofusesBuffer, foodBuffer).Schedule((int) doofusesBuffer.count,
                            (int) (doofusesBuffer.count / 8), inputDeps);

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
        readonly BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>> _doofuses;

        readonly BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<MealEntityStruct>> _food;

        public LookingForFoodDoofusesJob(
            in BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>> doofuses,
            in BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<MealEntityStruct>> food)
        {
            _doofuses = doofuses;
            _food = food;
        }

        public void Execute(int index)
        {
            float currentMin = float.MaxValue;

            ref var velocityEntityStruct = ref _doofuses.buffer2[index];
            ref var positionEntityStruct = ref _doofuses.buffer1[index];

            var foodcountLength = _food.count;

            float3 closerComputeDirection = default;
            for (int foodIndex = 0; foodIndex < foodcountLength; ++foodIndex)
            {
                var computeDirection = _food.buffer1[foodIndex].position - positionEntityStruct.position;

                var sqrModule = computeDirection.x * computeDirection.x + computeDirection.y * computeDirection.y +
                                computeDirection.z * computeDirection.z;

                if (currentMin > sqrModule)
                {
                    currentMin = sqrModule;
                    closerComputeDirection = computeDirection;

                    //food found
                    if (sqrModule < 2)
                    {
                        Interlocked.Increment(ref _food.buffer2[foodIndex].eaters);

                        closerComputeDirection = default;

                        break; //close enough let's save some computations
                    }
                }
            }

            //going toward food, not breaking as closer food can spawn
            velocityEntityStruct.velocity.x = closerComputeDirection.x;
            velocityEntityStruct.velocity.z = closerComputeDirection.z;
        }
    }
}