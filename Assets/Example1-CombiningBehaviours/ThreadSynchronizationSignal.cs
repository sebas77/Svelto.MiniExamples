using Svelto.Tasks.Enumerators;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class ThreadSynchronizationSignal : WaitForSignal<ThreadSynchronizationSignal>
    {
        public ThreadSynchronizationSignal(string name) : base(name, 1000, true, true)
        {
        }
    }
}