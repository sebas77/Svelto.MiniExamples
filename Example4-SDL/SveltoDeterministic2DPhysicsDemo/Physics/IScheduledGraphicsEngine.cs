using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IScheduledGraphicsEngine
    {
        string Name { get; }
        void   Draw(FixedPoint delta, ulong physicsTick);
    }
}