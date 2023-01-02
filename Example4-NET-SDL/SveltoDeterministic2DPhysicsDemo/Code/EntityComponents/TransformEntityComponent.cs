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

        public TransformEntityComponent(in FixedPointVector2 position, in FixedPointVector2 positionLastPhysicsTick)
        {
            Position                = position;
            PositionLastPhysicsTick = positionLastPhysicsTick;
            PositionMidPoint        = default;
            HasMidPoint             = false;
        }
    }

    static class TransformEntityComponentUtility
    {
        public static FixedPointVector2 Interpolate(this in TransformEntityComponent component, in FixedPoint delta)
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