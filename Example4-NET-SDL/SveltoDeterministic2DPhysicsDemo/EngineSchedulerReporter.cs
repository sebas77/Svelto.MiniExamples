using System.Collections.Generic;
using MiniExamples.DeterministicPhysicDemo.Graphics;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IEngineSchedulerReporter
    {
        void        RecordTicksSpent(string engine, long delta);
        void        IncrementFps();
        public void Report(IGraphics graphics);
        public void Reset();
    }

    public class EngineSchedulerReporter : IEngineSchedulerReporter
    {
        const int PtSize = 12;

        public void RecordTicksSpent(string engine, long delta)
        {
            if (!_ticksSpent.ContainsKey(engine))
                _ticksSpent[engine] = 0;

            _ticksSpent[engine] += delta;
        }

        public void IncrementFps()
        {
            _fpsAccumulator += 1;
        }

        public void Report(IGraphics graphics)
        {
            var row       = 0;
            var usedDelta = 0L;
            foreach (var (engine, delta) in _report)
            {
                graphics.DrawTextAbsolute(Colour.White, 0, row * (PtSize + 2)
                                        , $"{(float) delta / 1000:0000.0000}ms - {engine}");
                row       += 1;
                usedDelta += delta;
            }

            graphics.DrawTextAbsolute(Colour.White, 0, row * (PtSize + 2)
                                    , $"total = {(float) usedDelta / 1000:0000.0000}ms");
            graphics.DrawTextAbsolute(Colour.White, 0, (row + 1) * (PtSize + 2)
                                    , $"fps = {_fps}");
        }

        public void Reset()
        {
            _report         = _ticksSpent;
            _ticksSpent     = new Dictionary<string, long>();
            _fps            = _fpsAccumulator;
            _fpsAccumulator = 0;
        }

        Dictionary<string, long> _report     = new Dictionary<string, long>();
        Dictionary<string, long> _ticksSpent = new Dictionary<string, long>();
        uint                     _fpsAccumulator;
        uint                     _fps;
    }
}