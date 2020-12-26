using System;
using System.Diagnostics;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Graphics;

namespace MiniExamples.DeterministicPhysicDemo
{
    /// <summary>
    /// A simple experiment, not related to Svelto, to have a reusable GameLoop
    /// </summary>
    public class GameLoop : IGameLoop
    {
        const uint TicksPerMillisecond = 1000;
        const uint TicksPerSecond      = 1000 * TicksPerMillisecond;

        const uint  DefaultPhysicsSimulationsPerSecond = 30;
        const uint  DefaultGraphicsFramesPerSecond     = 60;
        const float DefaultSimulationSpeed             = 1.0f;

        public GameLoop()
        {
            SetPhysicsSimulationsPerSecond(DefaultPhysicsSimulationsPerSecond);
            SetGraphicsFramesPerSecond(DefaultGraphicsFramesPerSecond);
            SetSimulationSpeed(DefaultSimulationSpeed);
        }

        public IGameLoop AddGraphics(IGraphics graphics)
        {
            _graphics = graphics;
            return this;
        }

        public IGameLoop AddInput(IInput input)
        {
            _input = input;
            return this;
        }

        public void Execute()
        {
            if (!_graphics?.Init() ?? true)
            {
                Console.WriteLine("Graphics exist, but failed to init.");
                return;
            }

            var physicGroup = new ScheduledAction(
                () => _scheduler.ExecutePhysics(_physicsDeltaPerSimulation)

              , _physicsSimulationsPerSecond
              , true);

            var graphicsAction = new ScheduledAction(() =>
            {
                _graphics?.RenderStart();
                _scheduler.ExecuteGraphics(physicGroup.CalculateNormalisedDelta());

                _schedulerReporter.IncrementFps();
                if (_graphics != null)
                    _schedulerReporter.Report(_graphics);

                _graphics?.RenderEnd();
            }, _graphicsFramesPerSecond, false);

            var oncePerSecond = new ScheduledAction(() =>
            {
                if (_graphics != null)
                    _schedulerReporter.Reset();
            }, TicksPerSecond, true);

            var clock            = Stopwatch.StartNew();
            var lastElapsedTicks = clock.ElapsedTicks;
            var gameTick         = 0UL;
            _running             = true;
            
            while (_running)
            {
                _input?.PollEvents(this);

                // Calculate the time delta
                var elapsedTicks = clock.ElapsedTicks;
                gameTick         += (ulong) ((elapsedTicks - lastElapsedTicks) * _simulationSpeed);
                lastElapsedTicks =  elapsedTicks;

                // Execute graphic ticks
                graphicsAction.Tick((ulong) elapsedTicks);
                // Execute simulation ticks
                physicGroup.Tick(gameTick);
                oncePerSecond.Tick(gameTick);
            }

            _graphics?.Cleanup();
        }

        public IGameLoop SetGraphicsFramesPerSecond(uint frequency)
        {
            _graphicsFramesPerSecond = TicksPerSecond / frequency;
            return this;
        }

        public IGameLoop SetSchedulers(IEngineScheduler logicScheduler, IEngineSchedulerReporter logicSchedulerReporter)
        {
            _scheduler         = logicScheduler;
            _schedulerReporter = logicSchedulerReporter;

            return this;
        }

        public IGameLoop SetPhysicsSimulationsPerSecond(uint frequency)
        {
            _physicsSimulationsPerSecond = TicksPerSecond / frequency;
            _physicsDeltaPerSimulation   = FixedPoint.From(frequency);

            return this;
        }

        public IGameLoop SetSimulationSpeed(float simulationSpeed)
        {
            _simulationSpeed = simulationSpeed;
            return this;
        }

        public IGameLoop SetUncappedGraphicsFramesPerSecond()
        {
            _graphicsFramesPerSecond = 1; //TicksPerSecond;
            return this;
        }

        public void Stop() { _running = false; }

        IGraphics _graphics;
        uint      _graphicsFramesPerSecond;
        IInput    _input;

        uint                     _physicsSimulationsPerSecond;
        FixedPoint               _physicsDeltaPerSimulation = FixedPoint.Zero;
        bool                     _running;
        float                    _simulationSpeed;
        IEngineScheduler         _scheduler;
        IEngineSchedulerReporter _schedulerReporter;
    }
}