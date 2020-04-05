using Unity.Entities;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct UnityEcsEntityComponent : IEntityComponent
    {
        public Entity        uecsEntity;
        public float3        spawnPosition;
    }
}