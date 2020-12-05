﻿using System;
using Svelto.ECS;
 using Svelto.ECS.Schedulers;

 namespace SveltoDeterministic2DPhysicsDemo.Graphics
{
    public interface IInput
    {
        void PollEvents(IGameLoop gameLoop);
    }

    public interface IGameLoop
    {
        void Stop();
        void Execute();
        IGameLoop AddGraphics(IGraphics graphics);
        IGameLoop AddInput(IInput input);
        IGameLoop SetPhysicsSimulationsPerSecond(uint frequency);
        IGameLoop SetGraphicsFramesPerSecond(uint frequency);
        IGameLoop SetUncappedGraphicsFramesPerSecond();
        IGameLoop SetSimulationSpeed(float simulationSpeed);
        IGameLoop SetOnBeforeMainGameLoopAction(Action<IEntityFactory, SimpleEntitiesSubmissionScheduler> action);
    }
}