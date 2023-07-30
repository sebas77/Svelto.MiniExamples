using Unity.Mathematics;

namespace Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer
{
    public struct GameObjectEntityComponent : IEntityComponent
    {
        public int    prefabID;
        public float3 spawnPosition;
    }
}