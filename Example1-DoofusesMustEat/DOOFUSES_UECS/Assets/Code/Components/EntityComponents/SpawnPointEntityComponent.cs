using Unity.Entities;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    public readonly struct SpawnPointEntityComponent : IEntityComponent
    {
        public readonly bool   isSpecial;
        public readonly  Entity prefabEntity;
        public readonly  float3 spawnPosition;
        
        public SpawnPointEntityComponent(bool isSpecial, Entity prefabEntity, float3 spawnPosition)
        {
            this.isSpecial   = isSpecial;
            this.prefabEntity  = prefabEntity;
            this.spawnPosition = spawnPosition;
        }
    }
}