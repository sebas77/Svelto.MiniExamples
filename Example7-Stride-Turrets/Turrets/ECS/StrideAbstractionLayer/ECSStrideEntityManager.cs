using Stride.Engine;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ECSStrideEntityManager
    {
        public uint RegisterStrideEntity(Entity entity)
        {
            _entities.Add(entity);
            return _entityCount++;
        }

        public Entity GetStrideEntity(uint id)
        {
            return _entities[id];
        }

        uint                        _entityCount;
        readonly FasterList<Entity> _entities = new();
    }
}