using System;
using System.Diagnostics;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using SveltoDeterministic2DPhysicsDemo.Graphics;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics;
using SveltoDeterministic2DPhysicsDemo.Physics.Engines;

namespace SveltoDeterministic2DPhysicsDemo
{
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

            _schedulerReporter = new EngineSchedulerReporter();
            _scheduler         = new EngineScheduler(_schedulerReporter);
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
            _running = true;

            var clock = new Stopwatch();
            clock.Restart();

            EcsInit();
            if (!_graphics?.Init() ?? true)
            {
                Console.WriteLine("Graphics exist, but failed to init.");
                return;
            }

            _onBeforeMainGameLoop(_entityFactory, _simpleSubmissionEntityViewScheduler);

            var physicsAction = ScheduledAction.From(_scheduler.ExecutePhysics, _physicsSimulationsPerSecond, true);

            var graphicsAction = ScheduledAction.From(tick =>
            {
                _graphics?.RenderStart();
                _scheduler.ExecuteGraphics(
                    FixedPoint.From((float) physicsAction.RemainingDelta / _physicsSimulationsPerSecond)
                  , physicsAction.CurrentTick);

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

        public IGameLoop SetOnBeforeMainGameLoopAction(Action<IEntityFactory, SimpleEntitiesSubmissionScheduler> action)
        {
            _onBeforeMainGameLoop = action;
            return this;
        }

        public IGameLoop SetPhysicsSimulationsPerSecond(uint frequency)
        {
            _physicsSimulationsPerSecond           = TicksPerSecond / frequency;
            _physicsSimulationsPerSecondFixedPoint = FixedPoint.From(frequency);
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

        readonly EngineScheduler         _scheduler;
        readonly EngineSchedulerReporter _schedulerReporter;
        IEntityFactory                   _entityFactory;

        IGraphics _graphics;
        uint      _graphicsFramesPerSecond;
        IInput    _input;

        Action<IEntityFactory, SimpleEntitiesSubmissionScheduler> _onBeforeMainGameLoop = (factory, scheduler) => { };

        uint                              _physicsSimulationsPerSecond;
        FixedPoint                        _physicsSimulationsPerSecondFixedPoint;
        bool                              _running;
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
        float                             _simulationSpeed;

        void EcsInit()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            var enginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);

            _entityFactory = enginesRoot.GenerateEntityFactory();

            if (_graphics != null)
                enginesRoot.AddEngine(new DebugPhysicsDrawEngine(_scheduler, _graphics));
        }
    }
}