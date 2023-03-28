namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public struct ComputePositionComponent : IEntityComputeSharpComponent
    {
        public System.Numerics.Vector3    position;

        public ComputePositionComponent(System.Numerics.Vector3 transformPosition)
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