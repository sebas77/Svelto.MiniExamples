using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Debugger
{
    public class EnginesRootNamed : EnginesRoot
    {
        public string Name;
        public EnginesRootNamed(IEntitySubmissionScheduler entityComponentScheduler, string name) : base(entityComponentScheduler)
        {
            Name = name;
        }
    }
}