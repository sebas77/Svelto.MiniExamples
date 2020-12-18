using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct TransformEntityComponent : IEntityComponent
    {
        public FixedPointVector2 Position;
        public FixedPointVector2 PositionLastPhysicsTick;
        public FixedPointVector2? PositionMidpoint;

        public new string ToString() { return Position.ToString(); }

        TransformEntityComponent(FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick
                               , FixedPointVector2? positionMidpoint = null)
        {
            Position                = position;
            PositionLastPhysicsTick = positionLastPhysicsTick;
            PositionMidpoint       = positionMidpoint;
        }

        public static TransformEntityComponent From
        (FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick
       , FixedPointVector2? positionMidpoint = null)
        {
            return new TransformEntityComponent(position, positionLastPhysicsTick, positionMidpoint);
        }
    }

    static class TransformEntityComponentUtility
    {
        public static FixedPointVector2 Interpolate(this in TransformEntityComponent component, FixedPoint delta)
        {
            if (!component.PositionMidpoint.HasValue)
                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick, component.Position, delta);

            var magnitude = MathFixedPoint.Magnitude(component.Position - component.PositionMidpoint.Value);
            var lastMagnitude =
                MathFixedPoint.Magnitude(component.PositionLastPhysicsTick - component.PositionMidpoint.Value);
            var midPointFp = lastMagnitude / (magnitude + lastMagnitude + FixedPoint.Kludge);

            if (delta < midPointFp)
            {
                var midpointDeltaFp = (delta - midPointFp) / midPointFp + FixedPoint.One;

                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick
                                                   , component.PositionMidpoint.Value, midpointDeltaFp);
            }
            else
            {
                var midpointDeltaFp = (delta - midPointFp) / (FixedPoint.One - midPointFp);

                return FixedPointVector2.Interpolate(component.PositionMidpoint.Value, component.Position
                                                   , midpointDeltaFp);
            }
        }
    }
}