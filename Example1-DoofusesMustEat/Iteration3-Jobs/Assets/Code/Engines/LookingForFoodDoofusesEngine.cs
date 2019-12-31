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
            var doofuses =
                entitiesDB
                   .QueryEntities<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(GameGroups.DOOFUSES);

            var foods = entitiesDB.QueryEntities<PositionEntityStruct, MealEntityStruct>(GameGroups.FOOD);

            BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                NativeBuffer<HungerEntityStruct>> bufferTuple = doofuses
                                                               .ToNative<PositionEntityStruct, VelocityEntityStruct,
                                                                    HungerEntityStruct>().ToBuffers();
            BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<MealEntityStruct>> foodcount =
                foods.ToNative<PositionEntityStruct, MealEntityStruct>().ToBuffers();
            var deps = new LookingForFoodDoofusesJob(bufferTuple, foodcount).Schedule((int) doofuses.length,
                                                                                      (int) (doofuses.length / 8),
                                                                                      inputDeps);

            return
                JobHandle
                   .CombineDependencies(new DisposeJob<BufferTuple<NativeBuffer<PositionEntityStruct>, 
                NativeBuffer<VelocityEntityStruct>, NativeBuffer<HungerEntityStruct>>>(bufferTuple).Schedule(deps),
                                        new DisposeJob<BufferTuple<NativeBuffer<PositionEntityStruct>,
                                            NativeBuffer<MealEntityStruct>>>(foodcount).Schedule(deps));
        }
    }

    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
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

            var foodcountLength = _foodcount.length;

            float3 closerComputeDirection = default;
            for (int foodIndex = 0; foodIndex < foodcountLength; ++foodIndex)
            {
                var computeDirection = _foodcount.buffer1[foodIndex].position -
                                       positionEntityStruct.position;

                var sqrModule = computeDirection.x * computeDirection.x + computeDirection.y * computeDirection.y +
                                computeDirection.z * computeDirection.z;

                if (currentMin > sqrModule)
                {
                    currentMin = sqrModule;
                    closerComputeDirection = computeDirection;

                    //food found
                    if (sqrModule < 2)
                    {
                        hungerEntityStruct.hunger-=2;
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
#if noJobVersion
    [BurstCompile]
    public static class BurstIt
    {
        public delegate void LookingDelegate(in EntityCollection<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct> doofuses,
                                             in EntityCollection<PositionEntityStruct, MealEntityStruct> foodcount);
        
        public static readonly LookingDelegate functionToCompile =
 BurstCompiler.CompileFunctionPointer<LookingDelegate>(Burst).Invoke;
      //public static readonly LookingDelegate functionToCompile = Burst;
        /// <summary>
        /// Couldn't find a way to make it not unsafe yet
        /// </summary>
        [BurstCompile]
        static void Burst(
            in EntityCollection<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct> doofuses,
            in EntityCollection<PositionEntityStruct, MealEntityStruct>                         foodcount)
        {
            var doofusesLength = doofuses.Length;
            for (int doofusIndex = 0; doofusIndex < doofusesLength; ++doofusIndex)
            {
                float currentMin = float.MaxValue;
                
                ref var velocityEntityStruct = ref doofuses.Item2[doofusIndex];
                ref var hungerEntityStruct = ref doofuses.Item3[doofusIndex];
                ref var positionEntityStruct = ref doofuses.Item1[doofusIndex];

                var foodcountLength = foodcount.Length;
                for (int foodIndex = 0; foodIndex < foodcountLength; ++foodIndex)
                {
                    var computeDirection = foodcount.Item1[foodIndex].position - positionEntityStruct.position;
                   
                    var sqrModule = SqrMagnitude(computeDirection);

                    if (currentMin > sqrModule)
                    {
                        currentMin = sqrModule;

                        //food found
                        if (sqrModule < 10)
                        {
                            hungerEntityStruct.hunger--;
                            foodcount.Item2[foodIndex].eaters++;

                            break; //close enough let's save some computations
                        }
                        //going toward food, not breaking as closer food can spawn
                        velocityEntityStruct.velocity.x = computeDirection.x;
                        velocityEntityStruct.velocity.z = computeDirection.z;
                    }
                }

                hungerEntityStruct.hunger++;
            }
        }

        static float SqrMagnitude(in float3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }
        public static void WarmUp() { }
    }
#endif
}