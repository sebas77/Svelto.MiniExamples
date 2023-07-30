using Svelto.ECS.MiniExamples.Doofuses.StrideExample;

namespace DoofusesStride
{
    class DoofusesStrideApp
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
