using Svelto.ECS.MiniExamples.Turrets.Player;

namespace Svelto.ECS.MiniExamples.Turrets
{
    static class PlayerMock
    {
        static void Main(string[] args)
        {
            using (var game = new PlayerCompositionRootMock())
            {
                game.Run();
            }
        }
    }
}