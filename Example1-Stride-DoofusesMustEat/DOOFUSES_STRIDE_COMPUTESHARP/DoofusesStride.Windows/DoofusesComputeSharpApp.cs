using Svelto.ECS.MiniExamples.Doofuses.StrideExample;

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
