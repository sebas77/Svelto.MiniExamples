using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.Stride
{
    public struct PositionComponent : IEntityComponent
    {
        public Vector3    position;

        public PositionComponent(Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
}