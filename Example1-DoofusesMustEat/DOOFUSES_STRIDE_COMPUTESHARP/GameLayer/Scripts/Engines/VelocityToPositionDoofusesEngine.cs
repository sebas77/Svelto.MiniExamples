using ComputeSharp;
using Svelto.Common;
using Svelto.ECS.EntityComponents;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine: IQueryingEntitiesEngine, IUpdateEngine
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(VelocityToPositionDoofusesEngine);

        public void Step(in float deltaTime)
        {
            var doofusesEntityGroups =
                    entitiesDB.QueryEntities<ComputePositionComponent, ComputeVelocityComponent, ComputeSpeedComponent>(
                        GameGroups.DOOFUSES_EATING.Groups);

            foreach (var ((positions, velocities, speeds, count), _) in doofusesEntityGroups)
            {
                GraphicsDevice.GetDefault().For(
                    count,
                    new ComputePostionFromVelocityJob(
                        (positions.ToComputeBuffer(), velocities.ToComputeBuffer(), speeds.ToComputeBuffer()), deltaTime));

                positions.Update();
            }
        }
    }

    [AutoConstructor]
    readonly partial struct ComputePostionFromVelocityJob: IComputeShader
    {
        public ComputePostionFromVelocityJob(
            (ReadWriteBuffer<ComputePositionComponent> positions, ReadWriteBuffer<ComputeVelocityComponent> velocities,
                    ReadWriteBuffer<ComputeSpeedComponent> speeds)
                    doofuses,
            float deltaTime)
        {
            _positions = doofuses.positions;
            _velocities = doofuses.velocities;
            _speeds = doofuses.speeds;
            _deltaTime = deltaTime;
        }

        public void Execute()
        {
            var index = ThreadIds.X;

            var distance = _deltaTime * _speeds[index].speed;
            var velocity = _velocities[index].velocity;

            _positions[index].position += (velocity * distance);
        }

        readonly float _deltaTime;

        readonly ReadWriteBuffer<ComputePositionComponent> _positions;
        readonly ReadWriteBuffer<ComputeVelocityComponent> _velocities;
        readonly ReadWriteBuffer<ComputeSpeedComponent> _speeds;
    }
}