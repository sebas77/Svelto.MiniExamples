using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Maths;

namespace SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents
{
    public readonly struct RigidbodyEntityComponent : IEntityComponent
    {
        public static RigidbodyEntityComponent From
            (FixedPointVector2 direction, FixedPoint speed, FixedPoint restitution, FixedPoint mass, bool isKinematic)
        {
            return new RigidbodyEntityComponent(speed, direction, FixedPointVector2.Zero, FixedPoint.Zero, restitution
                                              , mass, isKinematic);
        }

        public readonly bool              IsKinematic;
        public readonly FixedPointVector2 Direction;
        public readonly FixedPointVector2 Velocity;
        public readonly FixedPoint        Speed;
        public readonly FixedPoint        Potential;
        public readonly FixedPoint        Restitution;
        public readonly FixedPoint        Mass;
        public readonly FixedPoint        InverseMass;

        RigidbodyEntityComponent
        (FixedPoint speed, FixedPointVector2 direction, FixedPointVector2 velocity, FixedPoint potential
       , FixedPoint restitution, FixedPoint mass, bool isKinematic)
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

        public RigidbodyEntityComponent CloneAndReplaceDirection(FixedPointVector2 direction)
        {
            return new RigidbodyEntityComponent(Speed, direction, Velocity, Potential, Restitution, Mass, IsKinematic);
        }

        public RigidbodyEntityComponent CloneAndReplacePotential(FixedPoint potential)
        {
            return new RigidbodyEntityComponent(Speed, Direction, Velocity, potential, Restitution, Mass, IsKinematic);
        }

        public RigidbodyEntityComponent CloneAndReplaceSpeed(FixedPoint speed)
        {
            return new RigidbodyEntityComponent(speed, Direction, Velocity, Potential, Restitution, Mass, IsKinematic);
        }

        public RigidbodyEntityComponent CloneAndReplaceVelocityAndPotential
            (FixedPointVector2 velocity, FixedPoint potential)
        {
            return new RigidbodyEntityComponent(Speed, Direction, velocity, potential, Restitution, Mass, IsKinematic);
        }

        public RigidbodyEntityComponent CloneAndReplaceVelocity(FixedPointVector2 velocity)
        {
            return new RigidbodyEntityComponent(Speed, Direction, velocity, Potential, Restitution, Mass, IsKinematic);
        }
    }
}