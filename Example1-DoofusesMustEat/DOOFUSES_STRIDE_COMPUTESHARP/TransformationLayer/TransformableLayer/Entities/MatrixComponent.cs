using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct ComputeMatrixComponent : IEntityComputeSharpComponent
    {
        public System.Numerics.Matrix4x4 matrix;
    }
    
    public struct MatrixComponent : IEntityComponent
    {
        public Matrix matrix;
    }
}