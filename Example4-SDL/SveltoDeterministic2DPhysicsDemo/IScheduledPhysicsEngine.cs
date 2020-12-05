namespace SveltoDeterministic2DPhysicsDemo
{
    public interface IScheduledPhysicsEngine
    {
        string Name { get; }
        void   Execute(ulong tick);
    }
}