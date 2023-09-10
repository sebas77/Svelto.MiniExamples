using System.Runtime.InteropServices;
using Stride.Core.Mathematics;
using Svelto.ECS;

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