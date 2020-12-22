using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct CircleColliderEntityComponent : IEntityComponent
    {
        public FixedPoint        Radius;
        public FixedPointVector2 Center;

        public CircleColliderEntityComponent(FixedPoint radius, FixedPointVector2 center)
        {
            Radius = radius;
            Center = center;
        }
    }
}