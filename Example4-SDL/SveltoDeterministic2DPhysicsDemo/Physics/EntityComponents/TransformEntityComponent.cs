using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct TransformEntityComponent : IEntityComponent
    {
        public FixedPointVector2 Position;
        public FixedPointVector2 PositionLastPhysicsTick;
        public FixedPointVector2 PositionMidPoint;
        public bool              HasMidPoint;

        public new string ToString() { return Position.ToString(); }

<<<<<<< HEAD
        public TransformEntityComponent(FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick)
=======
        public TransformEntityComponent(FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick
                               , FixedPointVector2? positionMidpoint = null)
>>>>>>> b7d0192... supports multiple collisions via entity creation
        {
            Position                = position;
            PositionLastPhysicsTick = positionLastPhysicsTick;
            PositionMidPoint        = default;
            HasMidPoint             = false;
        }
<<<<<<< HEAD
        
        public TransformEntityComponent(FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick
                               , FixedPointVector2 positionMidPoint)
        {
            Position                = position;
            PositionLastPhysicsTick = positionLastPhysicsTick;
            PositionMidPoint        = positionMidPoint;
            HasMidPoint             = true;
        }
=======
>>>>>>> b7d0192... supports multiple collisions via entity creation
    }

    static class TransformEntityComponentUtility
    {
        public static FixedPointVector2 Interpolate(this in TransformEntityComponent component, FixedPoint delta)
        {
            if (!component.HasMidPoint)
                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick, component.Position, delta);

            var magnitude = MathFixedPoint.Magnitude(component.Position - component.PositionMidPoint);
            var lastMagnitude =
                MathFixedPoint.Magnitude(component.PositionLastPhysicsTick - component.PositionMidPoint);
            var midPointFp = lastMagnitude / (magnitude + lastMagnitude + FixedPoint.Kludge);

            if (delta < midPointFp)
            {
                var midpointDeltaFp = (delta - midPointFp) / midPointFp + FixedPoint.One;

                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick
                                                   , component.PositionMidPoint, midpointDeltaFp);
            }
            else
            {
                var midpointDeltaFp = (delta - midPointFp) / (FixedPoint.One - midPointFp);

                return FixedPointVector2.Interpolate(component.PositionMidPoint, component.Position
                                                   , midpointDeltaFp);
            }
        }
    }
}