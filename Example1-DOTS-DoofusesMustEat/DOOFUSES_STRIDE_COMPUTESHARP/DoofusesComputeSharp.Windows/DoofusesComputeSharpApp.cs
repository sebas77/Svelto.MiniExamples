using Stride.Engine;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp;

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
