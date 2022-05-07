using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public struct PositionComponent : 
        //IEntityComputeSharpComponent
        IEntityComponent
    {
        public Vector3    position;

        public PositionComponent(Vector3 transformPosition)
        {
            position = transformPosition;
        }
    }
}