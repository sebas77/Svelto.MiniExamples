using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct PositionComponent : IEntityComponent
    {
        public Stride.Core.Mathematics.Vector3    position;

        public PositionComponent(Stride.Core.Mathematics.Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
}