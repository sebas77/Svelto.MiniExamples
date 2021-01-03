using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct GameObjectEntityComponent : IEntityComponent
    {
        public int    prefabID;
        public int    gameObjectID;
        public float3 spawnPosition;
    }
}