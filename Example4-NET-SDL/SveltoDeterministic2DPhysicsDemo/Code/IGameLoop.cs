using System;

namespace MiniExamples.DeterministicPhysicDemo.Graphics
{
    public interface IGameLoop
    {
        void      Stop();
        void      Execute();
        IGameLoop AddInput(IInput input);
        IGameLoop SetPhysicsSimulationsPerSecond(uint frequency);
        IGameLoop SetUncappedGraphicsFramesPerSecond();
        IGameLoop SetSimulationSpeed(float simulationSpeed);
        IGameLoop SetSchedulers(IEngineScheduler logicScheduler, IEngineSchedulerReporter logicSchedulerReporter = null);
    }
}