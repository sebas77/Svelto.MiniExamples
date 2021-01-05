using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct RigidbodyEntityComponent : IEntityComponent
    {
        public bool              IsKinematic;
        public FixedPointVector2 Direction;
        public FixedPointVector2 Velocity;
        public FixedPoint        Speed;
        public FixedPoint        Potential;
        public FixedPoint        Restitution;
        public FixedPoint        Mass;
        public FixedPoint        InverseMass;

        public RigidbodyEntityComponent
        (in FixedPoint speed, in FixedPointVector2 direction, in FixedPointVector2 velocity, in FixedPoint potential
       , in FixedPoint restitution, in FixedPoint mass, bool isKinematic)
        {
            Speed       = speed;
            Direction   = direction;
            Velocity    = velocity;
            Potential   = potential;
            Restitution = restitution;
            Mass        = mass;
            IsKinematic = isKinematic;
            InverseMass = FixedPoint.One / (mass + FixedPoint.Kludge);
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