using System;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public class ScheduledAction
    {
        public void Tick(ulong elapsedTicks)
        {
            _remainingDelta += elapsedTicks - _lastTick;

            if (_enforceFrequency)
            {
                while (_remainingDelta >= _frequency)
                {
                    _remainingDelta -= _frequency;

                    _action();
                }
            }
            else
            {
                if (_remainingDelta >= _frequency)
                {
                    var iterations = _remainingDelta / _frequency;

                    _remainingDelta -= _frequency * iterations;

                    _action();
                }
            }

            _lastTick = elapsedTicks;
        }

        public FixedPoint CalculateNormalisedDelta()
        {
            return FixedPoint.From((float) _remainingDelta / _frequency);
        }

        public ScheduledAction(Action action, ulong frequency, bool enforceFrequency)
        {
            _action           = action;
            _frequency        = frequency;
            _enforceFrequency = enforceFrequency;
            _lastTick         = 0UL;
            _remainingDelta   = 0UL;
        }
        
        readonly Action _action;
        readonly bool   _enforceFrequency;
        readonly ulong  _frequency;
        ulong           _lastTick;
        private ulong   _remainingDelta;
    }
}