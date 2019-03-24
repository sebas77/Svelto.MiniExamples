using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1B
{
    delegate T CompileDelegate<T>(T delegateMethod) where T : class;
    
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { SearchFoodOrGetHungry().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }

        
        
        IEnumerator SearchFoodOrGetHungry()
        {
/*            
            var ass  = Assembly.GetAssembly(typeof(BurstCompileAttribute));
            var type = ass.GetType("Unity.Burst.BurstCompiler");
            var prop = type.GetMethods();

            Delegate function;
            foreach (var method in prop)
            {
                if (method.Name == "CompileDelegate")
                {
                    Type funcType = typeof(Func<,>).MakeGenericType();
                    function = Delegate.CreateDelegate(funcType, null, method);
                }
            }
  */          
            unsafe void Execute()
            {
                var doofuses = entitiesDB
                     .QueryEntities<PositionEntityStruct, VelocityEntityStruct, HungerEntityStruct>(GameGroups.DOOFUSES,
                                                                                                      out var count);

                var foods =
                   entitiesDB.QueryEntities<PositionEntityStruct, MealEntityStruct>(GameGroups.FOOD, out var foodcount);
                
                var doofusesPosition    = GCHandle.Alloc(doofuses.Item1, GCHandleType.Pinned);
                var doofusesVelocity    = GCHandle.Alloc(doofuses.Item2, GCHandleType.Pinned);
                var foodPositions       = GCHandle.Alloc(foods.Item1, GCHandleType.Pinned);
                var mealStructs         = GCHandle.Alloc(foods.Item2, GCHandleType.Pinned);
                                                                                            var hungerEntityStructs = GCHandle.Alloc(doofuses.Item3, GCHandleType.Pinned);
                
                var dv = (VelocityEntityStruct *)doofusesVelocity.AddrOfPinnedObject();
                var hr = (HungerEntityStruct *)hungerEntityStructs.AddrOfPinnedObject();
                var ms = (MealEntityStruct *)mealStructs.AddrOfPinnedObject();
                var fp = (PositionEntityStruct *)foodPositions.AddrOfPinnedObject();
                var dp = (PositionEntityStruct *)doofusesPosition.AddrOfPinnedObject();

                    //                s.CompileDelegate(BurstIt);
                BurstIt.Burst((int) count, (int) foodcount, dv, hr, fp, dp, ms);

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
    public class BurstIt
    {

        [BurstCompile]
        public static unsafe void Burst(int count, int foodcount, VelocityEntityStruct* dv, HungerEntityStruct* hr, 
                                        PositionEntityStruct* fp, PositionEntityStruct* dp, MealEntityStruct* ms)
        {
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
}