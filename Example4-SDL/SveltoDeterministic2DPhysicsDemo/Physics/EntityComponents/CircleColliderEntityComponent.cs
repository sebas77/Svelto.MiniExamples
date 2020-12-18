using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct CircleColliderEntityComponent : IEntityComponent
    {
        public static CircleColliderEntityComponent From(FixedPoint radius, FixedPointVector2 center)
        {
            return new CircleColliderEntityComponent(radius, center);
        }

        public FixedPoint        Radius;
        public FixedPointVector2 Center;

        CircleColliderEntityComponent(FixedPoint radius, FixedPointVector2 center)
        {
            Radius = radius;
            Center = center;
        }
    }
}