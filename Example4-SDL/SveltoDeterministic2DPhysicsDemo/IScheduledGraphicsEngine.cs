using SveltoDeterministic2DPhysicsDemo.Maths;

namespace SveltoDeterministic2DPhysicsDemo
{
    public interface IScheduledGraphicsEngine
    {
        string Name { get; }
        void   Draw(FixedPoint delta, ulong physicsTick);
    }
}