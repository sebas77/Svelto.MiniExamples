using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    public struct GameObjectEntityComponent : IEntityComponent
    {
        public int    prefabID;
        public float3 spawnPosition;
    }
}