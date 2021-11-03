#if UNITY_ECS
using System.Collections;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface ISveltoUECSSubmission
    {
        void Add(SubmissionEngine engine);

        void SubmitEntities(JobHandle jobHandle);
        void SubmitEntitiesSliced(JobHandle jobHandle);
    }
}
#endif