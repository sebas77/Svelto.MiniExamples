using Stride.Engine;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class VelocityToPositionEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var groups = entitiesDB.FindGroups<VelocityComponent, PositionComponent>();
            foreach (var ((velocities, positions, count), _) in entitiesDB.QueryEntities<VelocityComponent, PositionComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    positions[i].position += velocities[i].velocity * deltaTime * 0.001f;
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
    }
}