using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Debugger
{
    public class EnginesRootNamed : EnginesRoot
    {
        public string Name;
        public EnginesRootNamed(IEntitySubmissionScheduler entityViewScheduler, string name) : base(entityViewScheduler)
        {
            Name = name;
        }
    }
}