using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        
        public string name => nameof(VelocityToPositionDoofusesEngine);

        public JobHandle Execute(JobHandle _jobHandle)
        {
            var doofusesEntityGroups =
                entitiesDB.QueryEntities<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>(   
                    GameGroups.DOOFUSES_EATING.Groups);

            foreach (var (doofuses, _) in doofusesEntityGroups)
            {
                var buffers = doofuses.ToBuffers();
                var dep = new ComputePostionFromVelocityJob(buffers, Time.deltaTime).ScheduleParallel(
                        buffers.count, _jobHandle);

                _jobHandle = JobHandle.CombineDependencies(_jobHandle, dep);
            }

            return _jobHandle;
        }

        readonly struct ComputePostionFromVelocityJob : IJobParallelFor
        {
            public ComputePostionFromVelocityJob(BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, 
                                                     NB<SpeedEntityComponent>> doofuses, float deltaTime)
            {
                _doofuses  = doofuses;
                _deltaTime = deltaTime;
            }

            public void Execute(int index)
            {
                var ecsVector3 = _doofuses.buffer2[index].velocity;

                _doofuses.buffer1[index].position += (ecsVector3 * (_deltaTime * _doofuses.buffer3[index].speed));
            }

            readonly float                                                                                  _deltaTime;
            readonly BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<SpeedEntityComponent>> _doofuses;
        }
    }
}