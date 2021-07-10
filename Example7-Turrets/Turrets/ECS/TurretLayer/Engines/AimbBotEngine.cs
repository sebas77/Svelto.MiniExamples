using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class AimbBotEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            var   updateTimeFrameCount = ((Game.UpdateTime.Total.TotalMilliseconds % 5000.0f) / 5000.0f) * Math.PI * 2;
            float x                    = (float) (0.3f * Math.Cos(updateTimeFrameCount));
            float y                    = (float) (0.3f * Math.Sin(updateTimeFrameCount));

            foreach (var ((trs, interpolatedPositions, count), _) in entitiesDB
               .QueryEntities<PositionComponent, StartPositionsComponent>(TurretTag.Groups))
            {
                for (int i = 0; i < count; i++)
                {
                    trs[i].position = interpolatedPositions[i].startPosition + new Vector3(x, 0, y);
                }
            }
        }
    }
}