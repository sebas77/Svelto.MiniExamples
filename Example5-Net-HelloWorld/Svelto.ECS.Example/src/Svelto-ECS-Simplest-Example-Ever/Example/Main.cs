using System.Threading;

namespace Svelto.ECS.Vanilla.Example
{
    public static class Program
    {
        static SimpleContext simpleContext;

        static void Main(string[] args)
        {
            simpleContext = new SimpleContext();

            while (true) Thread.Sleep(1000);
        }
    }
}