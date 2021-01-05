using System;
using FixedMaths;

namespace MiniExamples.DeterministicPhysicDemo
{
    public interface IScheduledPhysicsEngine
    {
        string Name { get; }
        void   Execute(in FixedPoint delta);
    }
}