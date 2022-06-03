using Stride.Core.Mathematics;

namespace  Svelto.ECS.MiniExamples.Doofuses.Stride
{
    public struct ScalingComponent : IEntityComponent
    {
        public Vector3 scaling;

        public ScalingComponent(Vector3 transformScale)
        {
            scaling = transformScale;
        }
    }
}