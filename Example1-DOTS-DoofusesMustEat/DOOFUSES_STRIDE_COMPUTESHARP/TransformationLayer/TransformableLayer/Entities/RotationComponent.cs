using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public struct RotationComponent : 
        //IEntityComputeSharpComponent
        IEntityComponent
    {
        public Quaternion rotation;

        public RotationComponent(Quaternion transformRotation)
        {
            rotation = transformRotation;
        }
    }
}