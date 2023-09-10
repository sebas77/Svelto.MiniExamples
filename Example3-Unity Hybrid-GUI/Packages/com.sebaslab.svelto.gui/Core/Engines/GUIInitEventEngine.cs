namespace Svelto.ECS.GUI.Engines
{
    class GUIInitEventEngine : IQueryingEntitiesEngine, IReactOnAdd<GUIFrameworkEventsComponent>
    {
        public GUIInitEventEngine(TriggeredEvents triggeredEvents)
        {
            _triggeredEvents = triggeredEvents;
        }

        public void Add(ref GUIFrameworkEventsComponent frameworkEvents, EGID egid)
        {
            if (frameworkEvents.map.ContainsKey(InitEventId))
            {
                _triggeredEvents.Add(new TriggeredEvent
                {
                    entity  = egid.ToEntityReference(entitiesDB),
                    eventId = InitEventId,
                    type = EventType.Framework
                });
            }
        }

        readonly TriggeredEvents _triggeredEvents;

        const int InitEventId = (int)FrameworkEvents.Init;

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
    }
}