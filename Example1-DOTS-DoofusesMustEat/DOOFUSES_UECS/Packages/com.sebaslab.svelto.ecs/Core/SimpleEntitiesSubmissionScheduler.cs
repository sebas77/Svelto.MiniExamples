using System;
using System.Collections.Generic;

namespace Svelto.ECS.Schedulers
{
    public sealed class SimpleEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        public SimpleEntitiesSubmissionScheduler(uint maxNumberOfOperationsPerFrame = UInt32.MaxValue)
        {
            _enumerator = SubmitEntitiesAsync(maxNumberOfOperationsPerFrame);
        }

        public override bool paused    { get; set; }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_onTick == null, "a scheduler can be exclusively used by one enginesRoot only");

                _onTick = value;
            }
        }

        public override void Dispose() { }

        public void SubmitEntities()
        {
            do
            {
                _enumerator.MoveNext();
            } while (_enumerator.Current == true);
        }

        public IEnumerator<bool> SubmitEntitiesAsync()
        {
            return _enumerator;
        }

        /// <summary>
        /// This method stays public in case the external code wants to handle the management of the pre allocated
        /// enumerator
        /// </summary>
        /// <param name="maxNumberOfOperations"></param>
        /// <returns></returns>
        public IEnumerator<bool> SubmitEntitiesAsync(uint maxNumberOfOperations)
        {
            EnginesRoot.EntitiesSubmitter entitiesSubmitter = _onTick.Value;
            entitiesSubmitter.maxNumberOfOperationsPerFrame = maxNumberOfOperations;

            while (true)
            {
                if (paused == false)
                {
                    var entitiesSubmitterSubmitEntities = entitiesSubmitter.submitEntities;
                    
                    entitiesSubmitterSubmitEntities.MoveNext();
                    
                    yield return entitiesSubmitterSubmitEntities.Current == true;
                }
            }
        }

        readonly IEnumerator<bool>     _enumerator;

        EnginesRoot.EntitiesSubmitter? _onTick;
    }
}