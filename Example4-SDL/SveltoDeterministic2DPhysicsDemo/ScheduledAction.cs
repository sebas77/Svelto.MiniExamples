using System;

namespace SveltoDeterministic2DPhysicsDemo
{
    public class ScheduledAction
    {
        public ulong CurrentTick    { get; private set; }
        public ulong RemainingDelta { get; private set; }

        public static ScheduledAction From(Action<ulong> action, ulong frequency, bool enforceFrequency)
        {
            return new ScheduledAction(action, frequency, enforceFrequency);
        }

        public void Tick(ulong elapsedTicks)
        {
            RemainingDelta += elapsedTicks - _lastTick;

            if (_enforceFrequency)
            {
                while (RemainingDelta >= _frequency)
                {
                    CurrentTick += 1;

                    RemainingDelta -= _frequency;

                    _action(CurrentTick);
                }
            }
            else
            {
                if (RemainingDelta >= _frequency)
                {
                    var iterations = RemainingDelta / _frequency;

                    CurrentTick += iterations;

                    RemainingDelta -= _frequency * iterations;

                    _action(CurrentTick);
                }
            }

            _lastTick = elapsedTicks;
        }

        readonly Action<ulong> _action;
        readonly bool          _enforceFrequency;
        readonly ulong         _frequency;
        ulong                  _lastTick;

        ScheduledAction(Action<ulong> action, ulong frequency, bool enforceFrequency)
        {
            _action           = action;
            _frequency        = frequency;
            _enforceFrequency = enforceFrequency;
            _lastTick         = 0UL;
            RemainingDelta    = 0UL;
            CurrentTick       = 0UL;
        }
    }
}