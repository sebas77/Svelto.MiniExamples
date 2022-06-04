using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Jobs;
using UnityEngine;

namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        
        public string name => nameof(VelocityToPositionDoofusesEngine);

        public JobHandle Execute(JobHandle inputDeps)
        {
            var doofusesEntityGroups =
                entitiesDB.QueryEntities<PositionEntityComponent, VelocityEntityComponent, SpeedEntityComponent>(   
                    GameGroups.DOOFUSES_EATING.Groups);

            JobHandle combined = inputDeps;

            foreach (var (doofuses, _) in doofusesEntityGroups)
            {
                var (buffer1, buffer2, buffer3, count) = doofuses;
                var dep = new ComputePostionFromVelocityJob((buffer1, buffer2, buffer3, count), Time.deltaTime).ScheduleParallel(
                        count, inputDeps);

                combined = JobHandle.CombineDependencies(combined, dep);
            }

            return combined;
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