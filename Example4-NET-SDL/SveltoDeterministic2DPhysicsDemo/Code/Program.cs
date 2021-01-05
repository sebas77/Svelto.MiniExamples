using MiniExamples.DeterministicPhysicDemo.Graphics;

namespace MiniExamples.DeterministicPhysicDemo
{
    /// <summary>
    /// Main entry, this is one of the many strategies possible to have a composition root. None of these strategies
    /// are tied to Svelto
    /// </summary>
    static class Program
    {
        static void Main()
        {
            var sdl2  = new Sdl2Driver();
            var logic = new CompositionRoot(sdl2);
            
            new GameLoop()
                .AddGraphics(sdl2)
                .AddInput(sdl2)
                .SetSchedulers(logic.scheduler, logic.schedulerReporter)
                .SetPhysicsSimulationsPerSecond(30)
                .SetSimulationSpeed(1.0f)
                .SetUncappedGraphicsFramesPerSecond()
                .Execute();
        }
    }
}