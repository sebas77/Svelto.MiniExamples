#if UNITY_2019_2_OR_NEWER
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IJobifiedEngine : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle);
    }
}
#endif