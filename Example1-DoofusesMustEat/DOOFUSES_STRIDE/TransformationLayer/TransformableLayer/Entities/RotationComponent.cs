using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct RotationComponent : IEntityComponent
    {
        public Quaternion rotation;

        public RotationComponent(Quaternion transformRotation)
        {
            rotation = transformRotation;
        }
    }
}