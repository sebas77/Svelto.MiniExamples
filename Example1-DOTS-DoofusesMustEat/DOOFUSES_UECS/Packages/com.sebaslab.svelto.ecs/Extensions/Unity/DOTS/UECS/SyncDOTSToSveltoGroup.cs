#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public class SyncDOTSToSveltoGroup : UnsortedJobifiedEnginesGroup<SyncDOTSToSveltoEngine>
    {}

    public abstract class SyncDOTSToSveltoEngine : SystemBase, IJobifiedEngine
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