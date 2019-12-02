using System;
using System.Collections;
using System.Runtime.InteropServices;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1B
{
#if !DISABLE_BURST
#if !BURST_WITH_FUNCTION    
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { SearchFoodOrGetHungry().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }
        
        IEnumerator SearchFoodOrGetHungry()
        {
            //careful this function is not safe to call outside jobs, but it works
            void Execute()
            {
                var doofuses = entitiesDB
                     .QueryEntities<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(GameGroups.DOOFUSES,
                                                                                                      out var count);

                var foods =
                   entitiesDB.QueryEntities<PositionEntityStruct, MealEntityStruct>(GameGroups.FOOD, out var foodcount);
                
                //this is not the correct way to use managed array inside burst functions, the
                //class NativeArrayUnsafeUtility must be used instead. This works just because
                //we are keeping the memory pinned
                var doofusesPosition    = GCHandle.Alloc(doofuses.Item1, GCHandleType.Pinned);
                var doofusesVelocity    = GCHandle.Alloc(doofuses.Item2, GCHandleType.Pinned);
                var foodPositions       = GCHandle.Alloc(foods.Item1, GCHandleType.Pinned);
                var mealStructs         = GCHandle.Alloc(foods.Item2, GCHandleType.Pinned);
                var hungerEntityStructs = GCHandle.Alloc(doofuses.Item3, GCHandleType.Pinned);
                
                var dv = doofusesVelocity.AddrOfPinnedObject();
                var hr = hungerEntityStructs.AddrOfPinnedObject();
                var ms = mealStructs.AddrOfPinnedObject();
                var fp = foodPositions.AddrOfPinnedObject();
                var dp = doofusesPosition.AddrOfPinnedObject();

                new BurstItOnMainThread((int) count, (int) foodcount, dv, hr, fp, dp, ms).Run();

                doofusesVelocity.Free();
                foodPositions.Free();
                mealStructs.Free();
                hungerEntityStructs.Free();
                doofusesPosition.Free();
            }

            while (entitiesDB.Count<PositionEntityStruct>(GameGroups.DOOFUSES) == 0)
                yield return null;

            while (true)
            {
                Execute();

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { private get; set; }
    }

    [BurstCompile]
    public struct BurstItOnMainThread:IJob
    {
        [NativeDisableUnsafePtrRestriction]
        IntPtr dpp;
        [NativeDisableUnsafePtrRestriction]
        IntPtr dvp;
        [NativeDisableUnsafePtrRestriction]
        IntPtr hrp;
        [NativeDisableUnsafePtrRestriction]
        IntPtr msp;
        [NativeDisableUnsafePtrRestriction]
        IntPtr fpp;
        int foodCount;
        int count;

        public BurstItOnMainThread(int count, int foodcount, IntPtr dvp, IntPtr hrp, IntPtr fpp, IntPtr dpp,
                                   IntPtr msp)
        {
            this.dvp = dvp;
            this.hrp = hrp;
            this.msp = msp;
            this.fpp = fpp;
            this.dpp = dpp;

            this.foodCount = foodcount;
            this.count = count;
        }

        public static float SqrMagnitude(in float3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }

        public void Execute()
        {
            unsafe
            {
                var dv = (VelocityEntityStruct *)dvp;
                var hr = (HungerEntityStruct *)hrp;
                var ms = (MealEntityStruct *)msp;
                var fp = (PositionEntityStruct *)fpp;
                var dp = (PositionEntityStruct *)dpp;
            
                for (int doofusIndex = 0; doofusIndex < count; doofusIndex++)
                {
                    float currentMin = float.MaxValue;
                
                    ref var velocityEntityStruct = ref dv[doofusIndex];
                    ref var hungerEntityStruct   = ref hr[doofusIndex];
                    ref var positionEntityStruct = ref dp[doofusIndex];

                    for (int foodIndex = 0; foodIndex < foodCount; foodIndex++)
                    {
                        var     computeDirection = fp[foodIndex];
                        ref var mealEntityStruct = ref ms[foodIndex];
                    
                        computeDirection.position -= positionEntityStruct.position;
                    
                        var sqrModule = SqrMagnitude(computeDirection.position);

                        if (currentMin > sqrModule)
                        {
                            currentMin = sqrModule;

                            //food found
                            if (sqrModule < 10)
                            {
                                hungerEntityStruct.hunger--;
                                mealEntityStruct.eaters++;

                                break; //close enough let's save some computations
                            }
                                //going toward food, not breaking as closer food can spawn
                                velocityEntityStruct.velocity.x = computeDirection.position.x;
                                velocityEntityStruct.velocity.z = computeDirection.position.z;
                        }
                    }

                    hungerEntityStruct.hunger++;
                }
            }
        }
    }
#else    
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { SearchFoodOrGetHungry().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }
        
        IEnumerator SearchFoodOrGetHungry()
        {
            //careful this function is not safe to call outside jobs, but it works
            var function = BurstCompiler.CompileFunctionPointer(BurstIt.functionToCompile).Invoke;

            void Execute()
            {
                var doofuses = entitiesDB
                     .QueryEntities<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(GameGroups.DOOFUSES,
                                                                                                      out var count);

                var foods =
                   entitiesDB.QueryEntities<PositionEntityStruct, MealEntityStruct>(GameGroups.FOOD, out var foodcount);
                
                //this is not the correct way to use managed array inside burst functions, the
                //class NativeArrayUnsafeUtility must be used instead. This works just because
                //we are keeping the memory pinned
                var doofusesPosition    = GCHandle.Alloc(doofuses.Item1, GCHandleType.Pinned);
                var doofusesVelocity    = GCHandle.Alloc(doofuses.Item2, GCHandleType.Pinned);
                var foodPositions       = GCHandle.Alloc(foods.Item1, GCHandleType.Pinned);
                var mealStructs         = GCHandle.Alloc(foods.Item2, GCHandleType.Pinned);
                var hungerEntityStructs = GCHandle.Alloc(doofuses.Item3, GCHandleType.Pinned);
                
                var dv = doofusesVelocity.AddrOfPinnedObject();
                var hr = hungerEntityStructs.AddrOfPinnedObject();
                var ms = mealStructs.AddrOfPinnedObject();
                var fp = foodPositions.AddrOfPinnedObject();
                var dp = doofusesPosition.AddrOfPinnedObject();

                function((int) count, (int) foodcount, dv, hr, fp, dp, ms);

                doofusesVelocity.Free();
                foodPositions.Free();
                mealStructs.Free();
                hungerEntityStructs.Free();
                doofusesPosition.Free();
            }

            while (entitiesDB.Count<PositionEntityStruct>(GameGroups.DOOFUSES) == 0)
                yield return null;

            while (true)
            {
                Execute();

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { private get; set; }
    }

    [BurstCompile]
    public static class BurstIt
    {
        public delegate void LookingDelegate(int count, int foodcount, IntPtr dvp, IntPtr hrp, IntPtr fpp, IntPtr dpp,
            IntPtr msp);
        
        public static readonly LookingDelegate functionToCompile = Burst;
        /// <summary>
        /// Couldn't find a way to make it not unsafe yet
        /// </summary>
        [BurstCompile]
        static unsafe void Burst(int count, int foodcount, IntPtr dvp, IntPtr hrp, IntPtr fpp, IntPtr dpp,
            IntPtr msp)
        {
            var dv = (VelocityEntityStruct *)dvp;
            var hr = (HungerEntityStruct *)hrp;
            var ms = (MealEntityStruct *)msp;
            var fp = (PositionEntityStruct *)fpp;
            var dp = (PositionEntityStruct *)dpp;
            
            for (int doofusIndex = 0; doofusIndex < count; doofusIndex++)
            {
                float currentMin = float.MaxValue;
                
                ref var velocityEntityStruct = ref dv[doofusIndex];
                velocityEntityStruct.velocity = new float3();
                ref var hungerEntityStruct = ref hr[doofusIndex];
                ref var positionEntityStruct = ref dp[doofusIndex];

                for (int foodIndex = 0; foodIndex < foodcount; foodIndex++)
                {
                    var computeDirection = fp[foodIndex];
                    ref var mealEntityStruct = ref ms[foodIndex];
                    
                    computeDirection.position -= positionEntityStruct.position;
                    
                    var sqrModule = SqrMagnitude(computeDirection.position);

                    if (currentMin > sqrModule)
                    {
                        currentMin = sqrModule;

                        //food found
                        if (sqrModule < 10)
                        {
                            hungerEntityStruct.hunger--;
                            mealEntityStruct.eaters++;

                 //           break; //close enough let's save some computations
                        }
                        else
                            //going toward food, not breaking as closer food can spawn
                        {
                            velocityEntityStruct.velocity.x = computeDirection.position.x;
                            velocityEntityStruct.velocity.z = computeDirection.position.z;
                        }
                    }
                }

                hungerEntityStruct.hunger++;
            }
        }

        public static float SqrMagnitude(in float3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }
    }
#endif    
#else
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { SearchFoodOrGetHungry().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }
        
        IEnumerator SearchFoodOrGetHungry()
        {
            void Execute()
            {
                var doofuses = entitiesDB
                     .QueryEntities<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(GameGroups.DOOFUSES,
                                                                                                      out var count);

                var foods =
                   entitiesDB.QueryEntities<PositionEntityStruct, MealEntityStruct>(GameGroups.FOOD, out var foodcount);
                
                NoBurstIt.NoBurst((int) count, (int) foodcount, doofuses.Item2, doofuses.Item3, foods.Item1, doofuses.Item1, foods.Item2);
            }

            while (entitiesDB.Count<PositionEntityStruct>(GameGroups.DOOFUSES) == 0)
                yield return null;

            while (true)
            {
                Execute();

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { private get; set; }
    }

    public class NoBurstIt
    {
        /// <summary>
        /// Couldn't find a way to make it not unsafe yet
        /// </summary>
        public static unsafe void NoBurst(int count, int foodcount, VelocityEntityStruct[] dvp,
            HungerEntityStruct[] hrp, PositionEntityStruct[] fpp, PositionEntityStruct[] dpp,
            MealEntityStruct[] msp)
        {
            var dv = dvp;
            var hr = hrp;
            var ms = msp;
            var fp = fpp;
            var dp = dpp;
            
            for (int doofusIndex = 0; doofusIndex < count; doofusIndex++)
            {
                float currentMin = float.MaxValue;
                
                ref var velocityEntityStruct = ref dv[doofusIndex];
                velocityEntityStruct.velocity = new float3();
                ref var hungerEntityStruct = ref hr[doofusIndex];
                ref var positionEntityStruct = ref dp[doofusIndex];

                for (int foodIndex = 0; foodIndex < foodcount; foodIndex++)
                {
                    var computeDirection = fp[foodIndex];
                    ref var mealEntityStruct = ref ms[foodIndex];
                    
                    computeDirection.position -= positionEntityStruct.position;
                    
                    var sqrModule = SqrMagnitude(computeDirection.position);

                    if (currentMin > sqrModule)
                    {
                        currentMin = sqrModule;

                        //food found
                        if (sqrModule < 10)
                        {
                            hungerEntityStruct.hunger--;
                            mealEntityStruct.eaters++;

                 //           break; //close enough let's save some computations
                        }
                        else
                            //going toward food, not breaking as closer food can spawn
                        {
                            velocityEntityStruct.velocity.x = computeDirection.position.x;
                            velocityEntityStruct.velocity.z = computeDirection.position.z;
                        }
                    }
                }

                hungerEntityStruct.hunger++;
            }
        }

        public static float SqrMagnitude(in float3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }
    }
#endif    
}