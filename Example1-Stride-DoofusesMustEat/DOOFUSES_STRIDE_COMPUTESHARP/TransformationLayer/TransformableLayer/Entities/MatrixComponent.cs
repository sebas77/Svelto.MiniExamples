using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [StructLayout(LayoutKind.Auto, Pack = 32)] //does the gpu benefit from this?
    public struct ComputeMatrixComponent : IEntityComputeSharpComponent
    {
        public Matrix matrix;
    }
    
    public struct MatrixComponent : IEntityComponent
    {
        public Matrix matrix;
    }
}