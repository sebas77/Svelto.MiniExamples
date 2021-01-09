using System;
using System.Collections.Generic;
using System.Diagnostics;
using FixedMaths;
using Svelto.ECS.Schedulers;

namespace MiniExamples.DeterministicPhysicDemo
{
    public class EngineScheduler : IEngineScheduler
    {
        public EngineScheduler
            (IEngineSchedulerReporter reporter, SimpleEntitiesSubmissionScheduler submissionScheduler)
        {
            _reporter                = reporter;
            _submissionScheduler     = submissionScheduler;
            _scheduledPhysicsEngines = new List<IScheduledPhysicsEngine>();
            _scheduledGraphicsEngine = new List<IScheduledGraphicsEngine>();
            _stopwatch               = Stopwatch.StartNew();
        }

        public void ExecuteGraphics(FixedPoint delta)
        {
            foreach (var engine in _scheduledGraphicsEngine)
            {
                var before = _stopwatch.ElapsedTicks;

                engine.Draw(delta);

                _reporter.RecordTicksSpent(engine.Name, _stopwatch.ElapsedTicks - before);
            }
        }

        public void ExecutePhysics(FixedPoint delta)
        {
            foreach (var engine in _scheduledPhysicsEngines)
            {
                var before = _stopwatch.ElapsedTicks;

                engine.Execute(delta);

                _reporter.RecordTicksSpent(engine.Name, _stopwatch.ElapsedTicks - before);
            }

            _submissionScheduler.SubmitEntities();
        }

        public void RegisterScheduledGraphicsEngine(IScheduledGraphicsEngine scheduledGraphicsEngine)
        {
            _scheduledGraphicsEngine.Add(scheduledGraphicsEngine);
        }

        public void RegisterScheduledPhysicsEngine(IScheduledPhysicsEngine scheduled)
        {
            _scheduledPhysicsEngines.Add(scheduled);
        }

        readonly IEngineSchedulerReporter                   _reporter;
        readonly SimpleEntitiesSubmissionScheduler  _submissionScheduler;
        readonly List<IScheduledGraphicsEngine>             _scheduledGraphicsEngine;
        readonly List<IScheduledPhysicsEngine>              _scheduledPhysicsEngines;
        readonly Stopwatch                                  _stopwatch;
    }
}