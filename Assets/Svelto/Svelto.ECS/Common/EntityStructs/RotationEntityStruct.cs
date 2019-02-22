using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct RotationEntityStruct : IEntityStruct
    {
        public ECSVector4 rotation;
        
        public EGID ID { get; set; }
    }
}