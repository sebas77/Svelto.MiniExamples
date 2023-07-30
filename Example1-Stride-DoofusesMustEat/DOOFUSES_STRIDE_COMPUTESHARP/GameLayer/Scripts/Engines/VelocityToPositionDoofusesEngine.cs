using System.Linq;
using ComputeSharp;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.ECS.EntityComponents;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [Sequenced(nameof(DoofusesEngineNames.VelocityToPositionDoofusesEngine))]
    public class VelocityToPositionDoofusesEngine: IQueryingEntitiesEngine, IUpdateEngine
    {
        public VelocityToPositionDoofusesEngine(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }
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
#if COMPUTE_SHADERS                         
                _graphicsDevice.For(
                    count,
                    new ComputePostionFromVelocityJob(
                        (positions.ToComputeBuffer(), velocities.ToComputeBuffer(), speeds.ToComputeBuffer()), deltaTime));

                positions.Update();
#else
                for (int index = 0; index < count; index++)
                {
                    float distance = deltaTime * speeds[index].speed;
                    var velocity = velocities[index].velocity;

                    Vector3 position = default;
                    position.X = velocity.X * distance;
                    position.Y = velocity.Y * distance;
                    position.Z = velocity.Z * distance;

                    var result = positions[index].position;

                    result.X += position.X;
                    result.Y += position.Y;
                    result.Z += position.Z;

                    positions[index].position = result;
                }
#endif                
            }
        }
        
        readonly GraphicsDevice _graphicsDevice;
    }

    [AutoConstructor]
    readonly partial struct ComputePostionFromVelocityJob: IComputeShader
    {
        public ComputePostionFromVelocityJob(
            (ReadWriteBuffer<ComputePositionComponent> positions, ReadWriteBuffer<ComputeVelocityComponent> velocities,
                    ReadWriteBuffer<ComputeSpeedComponent> speeds) doofuses, float deltaTime)
        {
            _positions = doofuses.positions;
            _velocities = doofuses.velocities;
            _speeds = doofuses.speeds;
            _deltaTime = deltaTime;
        }

        public void Execute()
        {
            var index = ThreadIds.X;

            float distance = _deltaTime * _speeds[index].speed;
            var velocity = _velocities[index].velocity;

            Vector3 position = default;
            position.X = velocity.X * distance;
            position.Y = velocity.Y * distance;
            position.Z = velocity.Z * distance;

            var result = _positions[index].position;

            result.X += position.X;
            result.Y += position.Y;
            result.Z += position.Z;

            _positions[index].position = result;
        }

        readonly float _deltaTime;

        readonly ReadWriteBuffer<ComputePositionComponent> _positions;
        readonly ReadWriteBuffer<ComputeVelocityComponent> _velocities;
        readonly ReadWriteBuffer<ComputeSpeedComponent> _speeds;
    }
}