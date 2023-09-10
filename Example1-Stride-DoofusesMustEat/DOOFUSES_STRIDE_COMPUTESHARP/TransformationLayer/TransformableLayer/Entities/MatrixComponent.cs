using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct ComputeMatrixComponent : IEntityComputeSharpComponent
    {
        public Matrix matrix;
    }
    
    public struct MatrixComponent : IEntityComponent
    {
        public Matrix matrix;
    }
}