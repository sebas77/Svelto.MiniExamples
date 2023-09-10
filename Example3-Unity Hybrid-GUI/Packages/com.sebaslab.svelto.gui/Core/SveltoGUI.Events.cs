using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.GUI.Commands;

namespace Svelto.ECS.GUI
{
    partial class SveltoGUI
    {
        internal void InitCustomEvents()
        {
            _nextEntityId = 0;
            _eventNameToId = new Dictionary<string, int>();
            _eventCommandsByName = new Dictionary<string, FasterList<SerializedCommandData>>();
            _triggeredEvents = new TriggeredEvents(16);
        }

        public int BuildCustomEvent(string eventName)
        {
            _nextEntityId++;
            var eventId = _nextEntityId;
            var egid = new EGID((uint)eventId, CustomEventsGroup);
            var entityInitializer = _entityFactory.BuildEntity<GUIEventEntityDescriptor>(egid);

            _eventNameToId.Add(eventName, eventId);

            if (_eventCommandsByName.TryGetValue(eventName, out var commands) && commands.count > 0)
            {
                var eventComponent = new GUICustomEventComponent();
                eventComponent.commandList = NativeDynamicArray.Alloc<CommandData>((uint)commands.count);

                for (uint i = 0; i < commands.count; i++)
                {
                    ref readonly var commandData = ref commands[i];
                    var command = new CommandData
                    {
                        id = _commandsManager.GetCommandId(commandData.commandName),
                        target = _resources.ToECS(commandData.target),
                        parameter1 = _resources.ToECS(commandData.parameter1),
                        parameter2 = _resources.ToECS(commandData.parameter2)
                    };
                    eventComponent.commandList.Add(command);
                }

                // Remove serialized command data.
                _eventCommandsByName.Remove(eventName);

                entityInitializer.Init(eventComponent);
            }
        #if DEBUG && !PROFILE_SVELTO
            else
            {
                Svelto.Console.LogWarning($"Building custom events {eventName} with 0 commands attached.");
                entityInitializer.Init(new GUICustomEventComponent
                {
                    commandList = NativeDynamicArray.Alloc<CommandData>()
                });
            }
        #endif

            return eventId;
        }

        public void AddCommandToCustomEvent(string eventName, string commandName, string target = ""
            , string parameter1 = "", string parameter2 = "")
        {
            DBC.Check.Require(_eventNameToId.ContainsKey(eventName) == false,
                "Event has already been built, this command will not be added.");
            if (_eventNameToId.ContainsKey(eventName)) return;

            if (_eventCommandsByName.TryGetValue(eventName, out var commandsList) == false)
            {
                commandsList = new FasterList<SerializedCommandData>(4);
                _eventCommandsByName[eventName] = commandsList;
            }

            commandsList.Add(new SerializedCommandData
            {
                commandName = commandName,
                target = target,
                parameter1 = parameter1,
                parameter2 = parameter2
            });
        }

        public void TriggerCustomEvent(int eventId, StructValue value = default)
        {
            _triggeredEvents.Add(new TriggeredEvent
            {
                eventId = eventId,
                value = value,
                type = EventType.Custom,
            });
        }

        public void TriggerWidgetEvent(EntityReference entity, int eventId, StructValue value = default)
        {
            DBC.Check.Require(entity != EntityReference.Invalid);
            _triggeredEvents.Add(new TriggeredEvent
            {
                entity = entity,
                eventId = eventId,
                type = EventType.Widget,
                value = value
            });
        }

        int                                                   _nextEntityId;
        Dictionary<string, FasterList<SerializedCommandData>> _eventCommandsByName;
        Dictionary<string, int>                               _eventNameToId;

        TriggeredEvents                                       _triggeredEvents;

        internal static ExclusiveGroup CustomEventsGroup = new ExclusiveGroup();
    }

    internal class TriggeredEvents
    {
        internal TriggeredEvents(int bufferSize)
        {
            currentFrameEvents = new FasterList<TriggeredEvent>(bufferSize);
            nextFrameEvents = new FasterList<TriggeredEvent>(bufferSize);
        }

        internal void GetTriggeredEvents(out TriggeredEvent[] events, out int count)
        {
            events = currentFrameEvents.ToArrayFast(out count);
            // Swap lists. Apparently this is the C# 7.0 way of swapping variables.
            (nextFrameEvents, currentFrameEvents) = (currentFrameEvents, nextFrameEvents);
            currentFrameEvents.Clear();
        }

        internal void Add(TriggeredEvent e)
        {
            currentFrameEvents.Add(e);
        }

        FasterList<TriggeredEvent> currentFrameEvents;
        FasterList<TriggeredEvent> nextFrameEvents;
    }
}