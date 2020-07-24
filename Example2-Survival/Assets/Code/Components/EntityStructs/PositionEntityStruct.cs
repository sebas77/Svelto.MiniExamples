using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct PositionEntityStruct : IEntityComponent
    {
        public ECSVector3 position;
    }
}