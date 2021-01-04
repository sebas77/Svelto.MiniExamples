using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    public struct GameObjectEntityComponent : IEntityComponent
    {
        public int    prefabID;
        public int    gameObjectID;
        public float3 spawnPosition;
    }
}