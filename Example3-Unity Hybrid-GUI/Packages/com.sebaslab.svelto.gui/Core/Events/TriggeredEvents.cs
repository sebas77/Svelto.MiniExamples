namespace Svelto.ECS.GUI
{
    internal struct TriggeredEvent
    {
        public EntityReference entity;
        public int             eventId;
        public StructValue     value;
        public EventType       type;
    }

    internal enum EventType
    {
        Framework,
        Widget,
        Custom
    }
}