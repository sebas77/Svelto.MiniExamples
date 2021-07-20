using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    static class Turrets
    {
        static void Main(string[] args)
        {
            using (var game = new TurretsCompositionRoot())
            {
                game.Run();
            }
        }
    }
}