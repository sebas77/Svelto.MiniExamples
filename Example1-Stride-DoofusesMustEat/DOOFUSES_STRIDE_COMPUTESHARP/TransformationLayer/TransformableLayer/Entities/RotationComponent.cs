using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [StructLayout(LayoutKind.Auto, Pack = 32)] //does the gpu benefit from this?
    public struct ComputeRotationComponent : IEntityComputeSharpComponent
    {
        public Quaternion rotation;

        public ComputeRotationComponent(Quaternion transformRotation)
        {
            rotation = transformRotation;
        }
    }
}