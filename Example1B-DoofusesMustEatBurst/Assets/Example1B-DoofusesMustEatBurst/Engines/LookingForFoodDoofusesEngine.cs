using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using MethodEmitter;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public static class DelegateHelper
    {
        private static readonly Type[] _DelegateCtorSignature = new Type[2]
        {
            typeof(object),
            typeof(IntPtr)
        };

        private static readonly Dictionary<DelegateKey, Type> DelegateTypes = new Dictionary<DelegateKey, Type>();

        public static Type NewDelegateType(Type ret, Type[] parameters)
        {
            var key = new DelegateKey(ret, (Type[])parameters.Clone());
            Type delegateType;
            if (!DelegateTypes.TryGetValue(key, out delegateType))
            {
                var assemblyName = Guid.NewGuid().ToString();

                var name = new AssemblyName(assemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name);
                assemblyBuilder.DefineVersionInfoResource();

                var typeBuilder = moduleBuilder.DefineType("CustomDelegate", System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed | System.Reflection.TypeAttributes.AutoClass, typeof(MulticastDelegate));
                var constructor = typeof(UnmanagedFunctionPointerAttribute).GetConstructors()[0];

                // Make sure that we setup the C calling convention on the unmanaged delegate
                var customAttribute = new CustomAttributeBuilder(constructor, new object[] { CallingConvention.Cdecl });
                typeBuilder.SetCustomAttribute(customAttribute);
                typeBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.HideBySig | System.Reflection.MethodAttributes.RTSpecialName, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(System.Reflection.MethodImplAttributes.CodeTypeMask);
                typeBuilder.DefineMethod("Invoke", System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.HideBySig | System.Reflection.MethodAttributes.VtableLayoutMask, ret, parameters).SetImplementationFlags(System.Reflection.MethodImplAttributes.CodeTypeMask);
                delegateType = typeBuilder.CreateType();

                DelegateTypes.Add(key, delegateType);
            }
            return delegateType;
        }

        private struct DelegateKey : IEquatable<DelegateKey>
        {
            public DelegateKey(Type returnType, Type[] arguments)
            {
                ReturnType = returnType;
                Arguments = arguments;
            }

            public readonly Type ReturnType;

            public readonly Type[] Arguments;

            public bool Equals(DelegateKey other)
            {
                if (ReturnType.Equals(other.ReturnType) && Arguments.Length == other.Arguments.Length)
                {
                    for (int i = 0; i < Arguments.Length; i++)
                    {
                        if (Arguments[i] != other.Arguments[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is DelegateKey && Equals((DelegateKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashcode = (ReturnType.GetHashCode() * 397) ^ Arguments.Length.GetHashCode();
                    for (int i = 0; i < Arguments.Length; i++)
                    {
                        hashcode = (hashcode * 397) ^ Arguments[i].GetHashCode();
                    }
                    return hashcode;
                }
            }
        }
    }
    
    delegate T CompileDelegate<T>(T delegateMethod) where T : class;
    
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine
    {
        public void Ready() { SearchFoodOrGetHungry().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }

        
        
        IEnumerator SearchFoodOrGetHungry()
        {
            T ConvertBurstMethodToDelegate<T>()
            {
                Delegate o;
                Delegate t = new Action<int, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>(BurstIt.Burst);
                o = BurstCompiler.CompileDelegate(t);
                T compile = DelegateCompiler.Compile<T>(o.Method);
                return compile;
            }

            var function = ConvertBurstMethodToDelegate<Action<int, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>>();
            
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
    public class BurstIt
    {

        [BurstCompile]
        public static unsafe void Burst(int count, int foodcount, IntPtr dvp, IntPtr hrp, 
                                        IntPtr fpp, IntPtr dpp, IntPtr msp)
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
}