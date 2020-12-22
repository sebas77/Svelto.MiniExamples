using Svelto.ECS.Components;
using Unity.Mathematics;

namespace Svelto.ECS.EntityStructs
{
    public struct VelocityEntityStruct : IEntityStruct
    {
        public float3 velocity;

        public EGID ID { get; set; }
    }
}    