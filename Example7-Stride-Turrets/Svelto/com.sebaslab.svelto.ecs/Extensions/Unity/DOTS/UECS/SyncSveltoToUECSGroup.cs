#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public class SyncSveltoToUECSGroup : UnsortedJobifiedEnginesGroup<SyncSveltoToUECSEngine>
    {
    }

    public abstract class SyncSveltoToUECSEngine : SystemBase, IJobifiedEngine
    {
        //The dependency returned is enough for the Svelto Engines running after this to take in consideration
        //the Systembase jobs. The svelto engines do not need to take in consideration the new dependencies created
        //by the World.Update because those are independent and are needed only by the next World.Update() jobs
        public JobHandle Execute(JobHandle inputDeps)
        {
            //SysteBase jobs that will use this Dependency will wait for inputDeps to be completed before to execute
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract    string name       { get; }
    }
}
#endif