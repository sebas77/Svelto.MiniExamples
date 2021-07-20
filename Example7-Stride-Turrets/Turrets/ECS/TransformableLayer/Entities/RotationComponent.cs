using Stride.Core.Mathematics;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public struct RotationComponent : IEntityComponent
    {
        public Quaternion rotation;

        public RotationComponent(Quaternion transformRotation)
        {
            rotation = transformRotation;
        }
    }
}