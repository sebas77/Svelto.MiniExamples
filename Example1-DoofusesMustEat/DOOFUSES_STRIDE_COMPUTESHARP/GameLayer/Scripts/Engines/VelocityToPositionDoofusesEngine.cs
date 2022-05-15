using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(VelocityToPositionDoofusesEngine);

        public void Step(in float deltaTime)
        {
            GroupsEnumerable<VelocityEntityComponent, SpeedEntityComponent> doofusesEntityGroups =
                entitiesDB.QueryEntities<VelocityEntityComponent, SpeedEntityComponent>(
                    GameGroups.DOOFUSES_EATING.Groups);

            foreach (var (doofuses, group) in doofusesEntityGroups)
            {
                var (positions, _)              = entitiesDB.QueryEntities<PositionComponent>(group);
                var (velocities, speeds, count) = doofuses;
                new ComputePostionFromVelocityJob((positions, velocities, speeds, count), deltaTime).Execute();
            }
        }

        readonly struct ComputePostionFromVelocityJob
        {
            public ComputePostionFromVelocityJob
            ((NB<PositionComponent> positions, NB<VelocityEntityComponent> velocities, NB<SpeedEntityComponent> speeds, int count) doofuses, float deltaTime)
            {
                _doofuses  = doofuses;
                _deltaTime = deltaTime;
            }

            public void Execute()
            {
                for (int index = 0; index < _doofuses.count; index++)
                {
                    ref var velocity = ref _doofuses.velocities[index].velocity;

                    var deltaPos = velocity * (_deltaTime * _doofuses.speeds[index].speed);

                    _doofuses.positions[index].position += deltaPos;
                }
            }

            readonly float _deltaTime;

            readonly (NB<PositionComponent> positions, NB<VelocityEntityComponent> velocities, NB<SpeedEntityComponent> speeds, int count) _doofuses;
        }
    }
}