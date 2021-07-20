using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class VelocityToPositionEngine : SyncScript, IQueryingEntitiesEngine
    {
        public override void Update()
        {
            var groups = entitiesDB.FindGroups<VelocityComponent, PositionComponent>();
            foreach (var ((velocities, positions, count), _) in entitiesDB.QueryEntities<VelocityComponent, PositionComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    positions[i].position += velocities[i].velocity * (float) Game.UpdateTime.Elapsed.TotalSeconds;
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
    }
}