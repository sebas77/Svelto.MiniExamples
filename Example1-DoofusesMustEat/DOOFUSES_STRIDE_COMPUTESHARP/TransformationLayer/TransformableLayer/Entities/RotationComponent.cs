using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct ComputeRotationComponent : IEntityComputeSharpComponent
    {
        public Quaternion rotation;

        public ComputeRotationComponent(Quaternion transformRotation)
        {
            rotation = transformRotation;
        }
    }
}