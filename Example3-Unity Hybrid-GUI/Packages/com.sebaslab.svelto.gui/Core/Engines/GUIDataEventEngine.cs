namespace Svelto.ECS.GUI.Engines
{
    class GUIDataEventEngine : IGUITickingEngine, IQueryingEntitiesEngine
    {
        public GUIDataEventEngine(TriggeredEvents triggeredEvents)
        {
            _triggeredEvents = triggeredEvents;
        }

        public void Tick()
        {
            // This is a GUI widget invalidation mechanism. Commands can set a isDirty flag on widgets that will cause
            // the framework to re-trigger the init event for the widget.
            var groups = entitiesDB.FindGroups<GUIComponent, GUIFrameworkEventsComponent>();
            var query = entitiesDB.QueryEntities<GUIComponent, GUIFrameworkEventsComponent>(groups);

            foreach (var ((guis, events, count), _) in query)
            {
                for (var i = 0; i < count; i++)
                {
                    if (guis[i].isDirty && events[i].map.isValid && events[i].map.ContainsKey(DataEventId))
                    {
                        _triggeredEvents.Add(new TriggeredEvent
                        {
                            entity = guis[i].selfReference,
                            eventId = DataEventId,
                            type = EventType.Framework
                        });

                        guis[i].isDirty = false;
                    }
                }
            }
        }

        readonly TriggeredEvents _triggeredEvents;

        const int DataEventId = (int)FrameworkEvents.Data;

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
    }
}