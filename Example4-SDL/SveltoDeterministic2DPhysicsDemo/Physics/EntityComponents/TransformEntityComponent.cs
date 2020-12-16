using Svelto.ECS;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct TransformEntityComponent : IEntityComponent
    {
        internal readonly FixedPointVector2? _positionMidpoint;

        public          FixedPointVector2 Position;
        public readonly FixedPointVector2 PositionLastPhysicsTick;

        public new string ToString() { return Position.ToString(); }

        TransformEntityComponent(FixedPointVector2 position, FixedPointVector2 positionLastPhysicsTick
                               , FixedPointVector2? positionMidpoint = null)
        {
            Position                = position;
            PositionLastPhysicsTick = positionLastPhysicsTick;
            _positionMidpoint       = positionMidpoint;
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
            if (!component._positionMidpoint.HasValue)
                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick, component.Position, delta);

            var magnitude = MathFixedPoint.Magnitude(component.Position - component._positionMidpoint.Value);
            var lastMagnitude =
                MathFixedPoint.Magnitude(component.PositionLastPhysicsTick - component._positionMidpoint.Value);
            var midPointFp = lastMagnitude / (magnitude + lastMagnitude + FixedPoint.Kludge);

            if (delta < midPointFp)
            {
                var midpointDeltaFp = (delta - midPointFp) / midPointFp + FixedPoint.One;

                return FixedPointVector2.Interpolate(component.PositionLastPhysicsTick
                                                   , component._positionMidpoint.Value, midpointDeltaFp);
            }
            else
            {
                var midpointDeltaFp = (delta - midPointFp) / (FixedPoint.One - midPointFp);

                return FixedPointVector2.Interpolate(component._positionMidpoint.Value, component.Position
                                                   , midpointDeltaFp);
            }
        }
    }
}