using System;
using Stride.Core.Mathematics;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// Move all the turret bases using a circular motion
    /// </summary>
    class MoveTurretEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            _totalTime += deltaTime;
            
            var   updateTimeFrameCount = ((_totalTime % 5000.0f) / 5000.0f) * Math.PI * 2.0f;
            float x                    = (float) (0.3f * Math.Cos(updateTimeFrameCount));
            float y                    = (float) (0.3f * Math.Sin(updateTimeFrameCount));

            foreach (var ((positions, startPositions, count), _) in entitiesDB
               .QueryEntities<PositionComponent, StartPositionsComponent>(TurretTag.Groups))
            {
                for (int i = 0; i < count; i++)
                {
                    positions[i].position = startPositions[i].startPosition + new Vector3(x, 0, y);
                }
            }
        }

        float _totalTime;
    }
}