using Stride.Core.Mathematics;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public struct TRSComponent : IEntityComponent
    {
        public Quaternion rotation;
        public Vector3    position;
        public Vector3    scaling;

        public TRSComponent(Vector3 transformPosition, Quaternion transformRotation, Vector3 transformScale)
        {
            position = transformPosition;
            rotation = transformRotation;
            scaling  = transformScale;
        }
    }
}