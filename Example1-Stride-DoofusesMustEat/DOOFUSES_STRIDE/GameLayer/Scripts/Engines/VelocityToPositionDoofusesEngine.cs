using Stride.Core.Mathematics;
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
                    entitiesDB.QueryEntities<PositionComponent, VelocityComponent, SpeedComponent>(
                        GameGroups.DOOFUSES_EATING.Groups);

            foreach (var ((positions, velocities, speeds, count), _) in doofusesEntityGroups)
            {
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
            }
        }
    }
}