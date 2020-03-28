#if UNITY_2019_2_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public static class UnityEntityDBExtensions
    {
        public static JobHandle CombineDispose
            <T1>(this in NativeBuffer<T1> buffer, JobHandle combinedDependencies,
                JobHandle                                               inputDeps)
            where T1 : unmanaged 
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                new DisposeJob<NativeBuffer<T1>>(buffer)
                    .Schedule(inputDeps));
        }
        
        public static JobHandle CombineDispose
            <T1>(this in EntityCollection<T1>.EntityNativeIterator<T1> entityCollection, JobHandle combinedDependencies,
                JobHandle                                               inputDeps)
            where T1 : unmanaged, IEntityComponent
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                new DisposeJob<EntityCollection<T1>.EntityNativeIterator<T1>>(entityCollection).Schedule(inputDeps));
        }
        
        public static JobHandle CombineDispose
            <T1, T2>(this in BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>> buffer, JobHandle combinedDependencies,
                     JobHandle                                               inputDeps)
            where T1 : unmanaged where T2 : unmanaged
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                                                 new DisposeJob<BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>>>(buffer)
                                                    .Schedule(inputDeps));
        }

        public static JobHandle CombineDispose
            <T1, T2, T3>(this in BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>, NativeBuffer<T3>> buffer,
                         JobHandle                                                                 combinedDependencies,
                         JobHandle                                                                 inputDeps)
            where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                                                 new DisposeJob<BufferTuple<NativeBuffer<T1>, NativeBuffer<T2>,
                                                     NativeBuffer<T3>>>(buffer).Schedule(inputDeps));
        }
        
        public static JobHandle CombineDispose<T1>(this in NativeEGIDMapper<T1> mapper, JobHandle combinedDependencies,
                                                    JobHandle inputDeps)
            where T1 : unmanaged, IEntityComponent
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                new DisposeJob<NativeEGIDMapper<T1>>(mapper).Schedule(inputDeps));
        }
    }
}
#endif