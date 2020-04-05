using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public void Ready()
        { }

        public EntitiesDB entitiesDB { get; set; }

        public JobHandle Execute(JobHandle _jobHandle)
        {
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var doofuses = entitiesDB
                    .QueryEntities<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>(group)
                    .ToNativeBuffers<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>();
                var dep = new ThisSystemJob(doofuses, Time.deltaTime).Schedule((int) doofuses.count,
                    (int) (doofuses.count / 8), _jobHandle);

                _jobHandle = new DisposeJob<BufferTuple<
                    NativeBuffer<PositionEntityComponent>, NativeBuffer<VelocityEntityComponent>,
                    NativeBuffer<SpeedEntityComponent>>>(doofuses).Schedule(dep);
            }

            return _jobHandle;
        }

        readonly struct ThisSystemJob : IJobParallelFor
        {
            public ThisSystemJob(
                BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<VelocityEntityComponent>,
                    NativeBuffer<SpeedEntityComponent>> doofuses, float deltaTime)
            {
                _doofuses = doofuses;
                _deltaTime = deltaTime;
            }

            public void Execute(int index)
            {
                var ecsVector3 = _doofuses.buffer2[index].velocity;

                _doofuses.buffer1[index].position += (ecsVector3 * (_deltaTime * _doofuses.buffer3[index].speed));
            }

            readonly float _deltaTime;
            readonly BufferTuple<NativeBuffer<PositionEntityComponent>, NativeBuffer<VelocityEntityComponent>,
                NativeBuffer<SpeedEntityComponent>> _doofuses;
        }
    }
}