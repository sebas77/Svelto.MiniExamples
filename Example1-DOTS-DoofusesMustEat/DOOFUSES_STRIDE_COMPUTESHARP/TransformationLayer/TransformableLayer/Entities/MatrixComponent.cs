using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public struct MatrixComponent : 
    //    IEntityComputeSharpComponent
    IEntityComponent
    {
        public Matrix matrix;
    }
}