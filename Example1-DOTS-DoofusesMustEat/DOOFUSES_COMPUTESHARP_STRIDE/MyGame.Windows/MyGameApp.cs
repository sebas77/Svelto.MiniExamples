using Stride.Engine;
using Svelto.ECS.MiniExamples.Turrets;

namespace MyGame
{
    class MyGameApp
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
