using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [StructLayout(LayoutKind.Auto, Pack = 32)] //does the gpu benefit from this?
    public struct ComputePositionComponent : IEntityComputeSharpComponent
    {
        public Stride.Core.Mathematics.Vector3    position;

        public ComputePositionComponent(Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
    
    public struct PositionComponent : IEntityComponent
    {
        public Stride.Core.Mathematics.Vector3    position;

        public PositionComponent(Stride.Core.Mathematics.Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
}