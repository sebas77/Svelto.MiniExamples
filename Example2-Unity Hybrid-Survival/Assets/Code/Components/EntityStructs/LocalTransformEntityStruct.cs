using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct LocalTransformEntityStruct : IEntityComponent
    {
        public ECSVector3 position;
        public ECSQuaternion rotation;
    }
}
