using Stride.Engine;
using Svelto.ECS.MiniExamples.Doofuses.Stride;

namespace DoofusesComputeSharp
{
    class DoofusesComputeSharpApp
    {
        static void Main(string[] args)
        {
            using (var game = new DoofusesCompositionRoot())
            {
                game.Run();
            }
        }
    }
}
