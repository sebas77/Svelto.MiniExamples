using System;
using Stride.Engine;
using Stride.Input;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    public static class PlayerContext
    {
        public static void Compose
        (Action<IEngine> AddEngine, InputManager input, ECSStrideEntityManager ecsStrideEntityManager
       , EnginesRoot enginesRoot, SceneSystem sceneSystem)
        {
            var entityFactory = enginesRoot.GenerateEntityFactory();

            AddEngine(new PlayerBotInputEngine(input));
            AddEngine(new BuildPlayerBotEngine(ecsStrideEntityManager, entityFactory, sceneSystem));
        }
    }
}