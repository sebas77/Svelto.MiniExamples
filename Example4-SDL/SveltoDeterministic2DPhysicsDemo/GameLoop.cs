using System;
using System.Diagnostics;
using MiniExamples.DeterministicPhysicDemo.Graphics;
using FixedMaths;

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
            
            _running = true;
            
            var clock = new Stopwatch();
            clock.Restart();

            _onBeforeMainGameLoop();
            
            var physicsSimulationsPerSecondFP = FixedPoint.From(_physicsSimulationsPerSecond / 1000);

            var physicsAction = ScheduledAction.From(
                (tick) => _scheduler.ExecutePhysics(physicsSimulationsPerSecondFP, tick), _physicsSimulationsPerSecond
              , true);

            var graphicsAction = ScheduledAction.From(tick =>
            {
                _graphics?.RenderStart();
         //       _scheduler.ExecuteGraphics((lastElapsedTicks - clock.ElapsedTicks) / 10000, tick);

                if (_graphics != null)
                    _schedulerReporter.Report(_graphics);

                _graphics?.RenderEnd();
            }, _graphicsFramesPerSecond, false);

            var perSecond = ScheduledAction.From(tick =>
            {
                if (_graphics != null)
                    _schedulerReporter.Reset();
            }, TicksPerSecond, true);

            var lastElapsedTicks = clock.ElapsedTicks;
            var gameTick         = 0UL;
            while (_running)
            {
                _input?.PollEvents(this);

                // Calculate the time delta
                var elapsedTicks = clock.ElapsedTicks;
                gameTick         += (ulong) ((elapsedTicks - lastElapsedTicks) * _simulationSpeed);
                lastElapsedTicks =  elapsedTicks;

                // Execute simulation ticks
                graphicsAction.Tick((ulong) elapsedTicks);
                physicsAction.Tick(gameTick);
                perSecond.Tick(gameTick);
            }

            _graphics?.Cleanup();
        }

        public IGameLoop SetGraphicsFramesPerSecond(uint frequency)
        {
            _graphicsFramesPerSecond = TicksPerSecond / frequency;
            return this;
        }

        public IGameLoop SetOnBeforeMainGameLoopAction(Action action)
        {
            _onBeforeMainGameLoop = action;
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
            _physicsSimulationsPerSecond           = TicksPerSecond / frequency;

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

        Action _onBeforeMainGameLoop = () => { };

        uint _physicsSimulationsPerSecond;

        bool                     _running;
        float                    _simulationSpeed;
        IEngineScheduler         _scheduler;
        IEngineSchedulerReporter _schedulerReporter;
    }
}