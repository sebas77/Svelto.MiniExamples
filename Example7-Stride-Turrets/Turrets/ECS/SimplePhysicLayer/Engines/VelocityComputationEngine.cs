using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class VelocityComputationEngine : SyncScript, IQueryingEntitiesEngine
    {
        public override void Update()
        {
            var groups = entitiesDB.FindGroups<VelocityComponent, DirectionComponent, SpeedComponent>();
            foreach (var ((velocities, directions, speeds, count), _) in entitiesDB
               .QueryEntities<VelocityComponent, DirectionComponent, SpeedComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    velocities[i].velocity = directions[i].vector * speeds[i].value;
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }
    }
}