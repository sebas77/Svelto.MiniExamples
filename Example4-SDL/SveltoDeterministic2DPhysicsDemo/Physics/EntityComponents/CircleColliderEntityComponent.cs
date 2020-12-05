using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Maths;

namespace SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents
{
    public readonly struct CircleColliderEntityComponent : IEntityComponent
    {
        public static CircleColliderEntityComponent From(FixedPoint radius, FixedPointVector2 center)
        {
            return new CircleColliderEntityComponent(radius, center);
        }

        public readonly FixedPoint        Radius;
        public readonly FixedPointVector2 Center;

        CircleColliderEntityComponent(FixedPoint radius, FixedPointVector2 center)
        {
            Radius = radius;
            Center = center;
        }
    }
}