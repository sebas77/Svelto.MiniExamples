using System.Collections.Generic;
using SveltoDeterministic2DPhysicsDemo.Graphics;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics.Loggers.Data;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Loggers
{
    public class FixedPointVector2Logger
    {
        static        FixedPointVector2Logger _instance;
        public static FixedPointVector2Logger Instance => _instance ??= new FixedPointVector2Logger();

        public readonly struct FixedPointVector2LoggerEntry
        {
            public readonly FixedPointVector2  Point;
            public readonly Colour             Colour;
            public readonly Shape              Shape;
            public readonly int?               Radius;
            public readonly FixedPointVector2? BoxMin;
            public readonly FixedPointVector2? BoxMax;
            public readonly ulong              RemovedAfterTick;

            public FixedPointVector2LoggerEntry
            (FixedPointVector2 point, Colour colour, Shape shape, int? radius, FixedPointVector2? min
           , FixedPointVector2? max, ulong removedAfterTick)
            {
                Point            = point;
                Colour           = colour;
                Shape            = shape;
                Radius           = radius;
                BoxMin           = min;
                BoxMax           = max;
                RemovedAfterTick = removedAfterTick;
            }
        }

        public void DrawBox
            (FixedPointVector2 min, FixedPointVector2 max, ulong tick, Colour colour, ulong duration = 10)
        {
            _events.Add(new FixedPointVector2LoggerEntry(FixedPointVector2.Zero, colour, Shape.Box, null, min, max
                                                       , tick + duration));
        }

        public void DrawCircle(FixedPointVector2 point, ulong tick, Colour colour, int radius, ulong duration = 10)
        {
            _events.Add(
                new FixedPointVector2LoggerEntry(point, colour, Shape.Circle, radius, null, null, tick + duration));
        }

        public void DrawCross(FixedPointVector2 point, ulong tick, Colour colour, int radius, ulong duration = 10)
        {
            _events.Add(
                new FixedPointVector2LoggerEntry(point, colour, Shape.Cross, radius, null, null, tick + duration));
        }

        public void DrawLine
            (FixedPointVector2 from, FixedPointVector2 to, ulong tick, Colour colour, ulong duration = 10)
        {
            _events.Add(new FixedPointVector2LoggerEntry(FixedPointVector2.Zero, colour, Shape.Line, null, from, to
                                                       , tick + duration));
        }

        public void DrawPlus(FixedPointVector2 point, ulong tick, Colour colour, int radius, ulong duration = 10)
        {
            _events.Add(
                new FixedPointVector2LoggerEntry(point, colour, Shape.Plus, radius, null, null, tick + duration));
        }

        public IEnumerable<FixedPointVector2LoggerEntry> GetPoints(ulong tick)
        {
            for (var i = _events.Count - 1; i >= 0; i--)
            {
                yield return _events[i];

                if (tick < _events[i].RemovedAfterTick)
                    continue;

                _events.RemoveAt(i);
            }
        }

        readonly List<FixedPointVector2LoggerEntry> _events = new List<FixedPointVector2LoggerEntry>();
    }
}