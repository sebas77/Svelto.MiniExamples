using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IScheduledPhysicsEngine
    {
        string Name { get; }
        void   Execute(FixedPoint delta);
    }
}