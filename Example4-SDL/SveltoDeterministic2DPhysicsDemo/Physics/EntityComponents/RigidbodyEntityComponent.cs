using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct RigidbodyEntityComponent : IEntityComponent
    {
        public static RigidbodyEntityComponent From
            (FixedPointVector2 direction, FixedPoint speed, FixedPoint restitution, FixedPoint mass, bool isKinematic)
        {
            return new RigidbodyEntityComponent(speed, direction, FixedPointVector2.Zero, FixedPoint.Zero, restitution
                                              , mass, isKinematic);
        }

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
        public static RigidbodyEntityComponent CloneAndReplaceDirection
            (this in RigidbodyEntityComponent component, FixedPointVector2 direction)
        {
            return new RigidbodyEntityComponent(component.Speed, direction, component.Velocity, component.Potential
                                              , component.Restitution, component.Mass, component.IsKinematic);
        }
    }
}