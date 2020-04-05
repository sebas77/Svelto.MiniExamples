#if UNITY_2019_1_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS.Extensions.Unity
{
    public abstract class JobifiableEnginesGroup<T, En>  where En:struct, ISequenceOrder where T: class, IJobifiableEngine
    {
        public JobifiableEnginesGroup(FasterReadOnlyList<T> engines)
        {
            _instancedSequence = new Sequence<T, En>(engines);
        }

        public JobHandle Execute(JobHandle combinedHandles)
        {
            var fasterReadOnlyList = _instancedSequence.items;
            for (var index = 0; index < fasterReadOnlyList.Count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles));
            }
            
            return combinedHandles;
        }

        readonly Sequence<T, En> _instancedSequence;
    }

    public interface IJobifiableEngine:IEngine
    {
        JobHandle Execute(JobHandle _jobHandle);
    }
}
#endif