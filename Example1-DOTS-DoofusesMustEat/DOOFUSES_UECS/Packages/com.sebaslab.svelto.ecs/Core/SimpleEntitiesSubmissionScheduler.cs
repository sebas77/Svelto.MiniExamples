namespace Svelto.ECS.Schedulers
{
    public sealed class SimpleEntitiesSubmissionScheduler : EntitiesSubmissionScheduler
    {
        public override bool paused    { get; set; }

        protected internal override EnginesRoot.EntitiesSubmitter onTick
        {
            set
            {
                DBC.ECS.Check.Require(_entitiesSubmitter == null, "a scheduler can be exclusively used by one enginesRoot only");

                _entitiesSubmitter = value;
            }
        }

        public override void Dispose() { }

        public void SubmitEntities()
        {
            _entitiesSubmitter.Value.SubmitEntities();
        }

        EnginesRoot.EntitiesSubmitter? _entitiesSubmitter;
    }
}