#if UNITY_ECS
using Svelto.Common;
using Svelto.DataStructures;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public class SyncDOTSToSveltoGroup : UnsortedJobifiedEnginesGroup<ISyncDOTSToSveltoEngine> {}

    public class SortedSyncDOTSToSveltoGroup<T_Order> : SortedJobifiedEnginesGroup<ISyncDOTSToSveltoEngine, T_Order>,
        ISyncDOTSToSveltoEngine where T_Order : struct, ISequenceOrder
    {
        public SortedSyncDOTSToSveltoGroup(FasterList<ISyncDOTSToSveltoEngine> engines) : base(engines)
        {
        }
    }

    public interface ISyncDOTSToSveltoEngine : IJobifiedEngine { }

    public abstract partial class SyncDOTSToSveltoEngine : SystemBase, ISyncDOTSToSveltoEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract string name { get; }
    }
    
    public abstract partial class SortedSyncDOTSToSveltoEngine : SystemBase, ISyncDOTSToSveltoEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract string name { get; }
    }
}
#endif