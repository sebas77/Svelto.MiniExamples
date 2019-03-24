using Svelto.ECS.Components.Unity;

namespace Svelto.ECS.EntityStructs
{
    public struct RotationEntityStruct : IEntityStruct
    {
        public ECSVector4 rotation;
        
        public EGID ID { get { return new EGID(); } set { } }
    }
}