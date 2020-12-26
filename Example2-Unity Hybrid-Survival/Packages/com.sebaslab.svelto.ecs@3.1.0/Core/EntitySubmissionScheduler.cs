using System.Collections;

namespace Svelto.ECS.Schedulers
{
    public abstract class EntitiesSubmissionScheduler
    {
        protected internal abstract EnginesRoot.EntitiesSubmitter onTick { set; }

        public abstract void Dispose();

        public abstract bool paused    { get; set; }
        public          uint iteration { get; protected internal set;  }
        
        internal bool isRunning;
    }
    
    public abstract class SimpleEntitiesSubmissionSchedulerInterface : EntitiesSubmissionScheduler
    {
        public void SubmitEntities()
        {
            var enumerator = SubmitEntitiesAsync();

            while (enumerator.MoveNext());
        }

        public abstract IEnumerator SubmitEntitiesAsync();
    }
}