using System;
using System.Collections;

namespace Svelto.ECS.Schedulers
{
    //This scheduler shouldn't be used in production and it's meant to be used for Unit Tests only
    public sealed class SimpleEntitiesSubmissionScheduler : SimpleEntitiesSubmissionSchedulerInterface
    {
        public SimpleEntitiesSubmissionScheduler(uint maxNumberOfOperationsPerFrame = UInt32.MaxValue)
        {
            _maxNumberOfOperationsPerFrame = maxNumberOfOperationsPerFrame;
        }
        
        public override IEnumerator SubmitEntitiesAsync()
        {
            if (paused == false)
            {
                var submitEntities = _onTick.Invoke(_maxNumberOfOperationsPerFrame);
                
                while (submitEntities.MoveNext())
                    yield return null;
            }
        }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_onTick.IsUnused, "a scheduler can be exclusively used by one enginesRoot only");
                
                _onTick = value;
            }
        }

        public override bool paused                        { get; set; }

        public override void Dispose() { }

        EnginesRoot.EntitiesSubmitter _onTick;
        readonly uint                 _maxNumberOfOperationsPerFrame;
    }
}