using Unity.Entities;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public readonly struct SpawnPointEntityComponent : IEntityComponent
    {
        public readonly Entity prefabEntity;
        public readonly float3 spawnPosition;
        
        public SpawnPointEntityComponent(Entity prefabEntity, float3 spawnPosition)
        {
            this.prefabEntity  = prefabEntity;
            this.spawnPosition = spawnPosition;
        }
    }
}