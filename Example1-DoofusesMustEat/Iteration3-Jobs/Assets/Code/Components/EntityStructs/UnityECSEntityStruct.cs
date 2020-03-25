using Unity.Entities;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct UnityEcsEntityStruct : IEntityComponent, INeedEGID
    {
        public Entity        uecsEntity;
        public float3        spawnPosition;

        public EGID ID { get; set; }
    }
}