using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Unity.Jobs;
using Unity.Mathematics;
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
            var groupsToUpdate = GroupCompound<GameGroups.DOOFUSES, GameGroups.EATING>.Groups;

            var doofusesEntityGroups =
                entitiesDB.NativeGroupsIterator<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>(
                    groupsToUpdate);
#if !OLD
            foreach (var doofuses in doofusesEntityGroups)    
            {
                var dep = new ComputePostionFromVelocityJob(doofuses, Time.deltaTime).Schedule((int) doofuses.count, 
                                                                    ProcessorCount.BatchSize(doofuses.count), _jobHandle);

                _jobHandle = doofuses.CombineDispose(dep, _jobHandle);
            }
#else            
            foreach (var group in groupsToUpdate)
            {
                var doofuses = entitiesDB
                    .QueryEntities<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>(group)
                    .ToNativeBuffers<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>();
                var dep = new ThisSystemJob(doofuses, Time.deltaTime).Schedule((int) doofuses.count,
                    ProcessorCount.Batch(doofuses.count), _jobHandle);

                _jobHandle = new DisposeJob<BT<
                    NB<PositionEntityComponent>, NB<VelocityEntityComponent>,
                    NB<SpeedEntityComponent>>>(doofuses).Schedule(dep);
            }
#endif
            return _jobHandle;
        }

        readonly struct ComputePostionFromVelocityJob : IJobParallelFor
        {
            public ComputePostionFromVelocityJob(BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>,
                                                     NB<SpeedEntityComponent>> doofuses, float deltaTime)
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
            readonly BT<NB<PositionEntityComponent>, NB<VelocityEntityComponent>, NB<SpeedEntityComponent>> _doofuses;
        }
    }
}