using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct VelocityEntityStruct : IEntityStruct
    {
        public ECSVector3 velocity;

        public EGID ID { get; set; }
    }
}    