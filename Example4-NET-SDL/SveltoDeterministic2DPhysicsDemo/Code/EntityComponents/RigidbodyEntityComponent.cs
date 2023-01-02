using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct RigidbodyEntityComponent : IEntityComponent
    {
        public FixedPointVector2 Direction;
        public FixedPointVector2 Velocity;
        public FixedPoint        Speed;
        public FixedPoint        Restitution;

        public RigidbodyEntityComponent
        (in FixedPoint speed, in FixedPointVector2 direction, in FixedPointVector2 velocity, in FixedPoint restitution)
        {
            Speed       = speed;
            Direction   = direction;
            Velocity    = velocity;
            Restitution = restitution;
        }
    }

    public static class RigidbodyEntityComponentUtility
    {
        public static void AddImpulse(this ref RigidbodyEntityComponent component, FixedPointVector2 impulse)
        {
            component.Direction = (component.Velocity - impulse).Normalize();
        }
    }
}