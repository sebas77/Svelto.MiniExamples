#if UNITY_2019_1_OR_NEWER
using System;
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS
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
                combinedHandles = engine.Execute(combinedHandles);
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