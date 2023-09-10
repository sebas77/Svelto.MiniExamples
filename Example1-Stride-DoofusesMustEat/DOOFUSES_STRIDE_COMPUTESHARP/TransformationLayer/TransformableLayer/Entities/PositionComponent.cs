using System.Runtime.InteropServices;
using Stride.Core.Mathematics;
using Svelto.ECS;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct ComputePositionComponent : IEntityComputeSharpComponent
    {
        public Vector3    position;

        public ComputePositionComponent(Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
    
    public struct PositionComponent : IEntityComponent
    {
        public Vector3    position;

        public PositionComponent(Stride.Core.Mathematics.Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
}