using System;
using Stride.Input;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    public static class PlayerContext
    {
        public static void Compose(Action<IEngine> AddEngine, InputManager input)
        {
            AddEngine(new PlayerBotInputEngine(input));
        }
    }
}