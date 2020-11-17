#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public class SyncSveltoToUECSGroup : JobifiedEnginesGroup<SyncSveltoToUECSEngine>
    {
    }

    public abstract class SyncSveltoToUECSEngine : SystemBase, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle _jobHandle)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, _jobHandle);
            
            Update();

            return Dependency;
        }

        public abstract    string name       { get; }
    }
}
#endif