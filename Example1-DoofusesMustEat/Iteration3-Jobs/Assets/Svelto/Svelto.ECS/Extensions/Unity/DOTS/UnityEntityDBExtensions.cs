#if UNITY_COLLECTIONS
using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Internal;
using Unity.Jobs;

namespace Svelto.ECS
{
    public static class UnityEntityDBExtensions
    {
        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this TypeSafeDictionary<T> dic,
            ExclusiveGroupStruct groupStructId) where T : unmanaged, IEntityComponent
        {
            var mapper = new NativeEGIDMapper<T>(groupStructId, dic.implUnmgd);

            return mapper;
        }

        public static JobHandle ScheduleDispose
            <T1>(this T1 disposable, JobHandle inputDeps) where T1 : struct, IDisposable
        {
            return new DisposeJob<T1>(disposable).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleDispose
            <T1, T2>(this T1 disposable1, T2 disposable2, JobHandle inputDeps) 
                where T1 : struct, IDisposable where T2 : struct, IDisposable
        {
            return new DisposeJob<T1, T2>(disposable1, disposable2).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleParallel
            <JOB>(this JOB job, uint iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            var innerloopBatchCount = ProcessorCount.BatchSize(iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeEGIDMapper<T> QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T));

            return (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper<T>(groupStructId);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeEGIDMultiMapper<T> QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, FasterReadOnlyList<ExclusiveGroupStruct> groups)
            where T : unmanaged, IEntityComponent
        {
            var dictionary =
                new SveltoDictionary<ExclusiveGroupStruct, SveltoDictionary<uint, T, NativeStrategy<FasterDictionaryNode<uint>>, NativeStrategy<T>>, 
                    NativeStrategy<FasterDictionaryNode<ExclusiveGroupStruct>>, NativeStrategy<SveltoDictionary<uint, T, NativeStrategy<FasterDictionaryNode<uint>>, NativeStrategy<T>>>> 
                    (groups.count, Allocator.TempJob);
        
            foreach (var group in groups)
            {
                if (entitiesDb.SafeQueryEntityDictionary<T>(group, out var typeSafeDictionary) == true)
                    dictionary.Add(group, ((TypeSafeDictionary<T>)typeSafeDictionary).implUnmgd);
            }
            
            
            
            return new NativeEGIDMultiMapper<T>(dictionary);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, FasterReadOnlyList<ExclusiveGroupStruct> groups, out NativeEGIDMultiMapper<T> mapper)
            where T : unmanaged, IEntityComponent
        {
            var dictionary =
                new SveltoDictionary<ExclusiveGroupStruct, SveltoDictionary<uint, T, NativeStrategy<FasterDictionaryNode<uint>>, NativeStrategy<T>>, 
                        NativeStrategy<FasterDictionaryNode<ExclusiveGroupStruct>>, NativeStrategy<SveltoDictionary<uint, T, NativeStrategy<FasterDictionaryNode<uint>>, NativeStrategy<T>>>> 
                    (groups.count, Allocator.TempJob);
        
            foreach (var group in groups)
            {
                if (entitiesDb.SafeQueryEntityDictionary<T>(group, out var typeSafeDictionary) == true)
                    dictionary.Add(group, ((TypeSafeDictionary<T>)typeSafeDictionary).implUnmgd);
            }
            
            mapper = new NativeEGIDMultiMapper<T>(dictionary);

            if (dictionary.count == 0)
                return false;

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId,
                                                           out NativeEGIDMapper<T> mapper)
            where T : unmanaged, IEntityComponent
        {
            mapper = default;
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false ||
                typeSafeDictionary.count == 0)
                return false;

            mapper = (typeSafeDictionary as TypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);

            return true;
        }

    }
}
#endif