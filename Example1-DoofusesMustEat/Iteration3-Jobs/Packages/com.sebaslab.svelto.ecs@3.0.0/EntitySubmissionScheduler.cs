using System;

namespace Svelto.ECS.Schedulers
{
    public interface IEntitiesSubmissionScheduler: IDisposable
    {
        EnginesRoot.EntitiesSubmitter onTick { set; }

        bool paused { get; set; }
    }
    
    public interface ISimpleEntitiesSubmissionScheduler: IEntitiesSubmissionScheduler
    {
        void SubmitEntities();
    }
}