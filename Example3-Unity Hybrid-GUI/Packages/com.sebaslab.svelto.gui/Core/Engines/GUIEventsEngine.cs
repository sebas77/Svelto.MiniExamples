#if !UNITY_EDITOR
using System;
#endif
using Svelto.ECS.GUI.Commands;
using Svelto.ECS.GUI.Resources;
using Svelto.Tasks.Lean;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Tasks;

namespace Svelto.ECS.GUI.Engines
{
    class GUIEventsEngine : IQueryingEntitiesEngine, IGUITickingEngine, IDisposingEngine
    {
        /// <summary>
        /// This engine is in charge of executing commands attached to GUI events (both widget and custom ones).
        /// It is not thread-safe.
        /// </summary>
        public GUIEventsEngine(CommandsManager commandsManager, GUIResources resources, GUIWidgetMap widgetMap
            , GUIBlackboard blackboard, TriggeredEvents triggeredEvents)
        {
            _commandsManager = commandsManager;
            _resources = resources;
            _widgetMap = widgetMap;
            _blackboard = blackboard;
            _commandRunner = new SteppableRunner("GUICustomEventsEngineRunner");
            _triggeredEvents = triggeredEvents;
        }

        public void Tick()
        {
            _triggeredEvents.GetTriggeredEvents(out var events, out var count);
            for (var eIndex = 0; eIndex < count; eIndex++)
            {
                ref readonly var eventTrigger = ref events[eIndex];

                // Get commands list for the event. There are different methods depending whether we are triggering a
                // custom event or a widget event.
                NativeDynamicArray commandList = default;
                GUIComponent gui = default;
                entitiesDB.TryGetEGID(eventTrigger.entity, out var egid);

                if (eventTrigger.type == EventType.Framework && entitiesDB.Exists<GUIComponent>(egid))
                {
                    // The framework should have already check whether there are commands for this widget before
                    // triggering the event.
                    var eventComponent = entitiesDB.QueryEntity<GUIFrameworkEventsComponent>(egid);
                    var found = eventComponent.map.TryGetValue(eventTrigger.eventId, out commandList);
                    DBC.Check.Require(found);

                    gui = entitiesDB.QueryEntity<GUIComponent>(egid);
                }
                else if (eventTrigger.type == EventType.Widget && entitiesDB.Exists<GUIComponent>(egid))
                {
                    var eventComponent = entitiesDB.QueryEntity<GUIWidgetEventsComponent>(egid);
                    // If there are no commands for the event the map isn't filled with this event id.
                    // So we must check if it exists.
                    if (eventComponent.map.TryGetValue(eventTrigger.eventId, out commandList) == false)
                    {
                        continue;
                    }

                    gui = entitiesDB.QueryEntity<GUIComponent>(egid);
                }
                // Otherwise this is a custom event.
                else if (eventTrigger.type == EventType.Custom)
                {
                    var eventEgid = new EGID((uint)eventTrigger.eventId, SveltoGUI.CustomEventsGroup);
                    if (entitiesDB.TryGetEntity(eventEgid, out GUICustomEventComponent eventComponent))
                    {
                        commandList = eventComponent.commandList;
                    }
                    // The custom event might not have been built yet skip this event for next frame.
                    else
                    {
                    #if SVELTO_GUI_DEBUG
                        Svelto.Console.LogWarning(
                            "SveltoGUI: triggering nonexistent custom event. This will be retried next frame.");
                    #endif
                        _triggeredEvents.Add(eventTrigger);
                        continue;
                    }
                }
                else
                {
                    // It means we are handling an event for a widget that has been removed. Probably belonging to a
                    // dynamic container.
                    continue;
                }

                DBC.Check.Require(commandList.isValid);
                TriggerCommands(commandList, eventTrigger.value, gui).RunOn(_commandRunner);
            }

            // We want to keep stepping the command runner as long as there are new commands being spawned, so we can
            // handle as much as possible in a single frame.
            do
            {
                _hasSpawnedCommandsThisFrame = false;
                _commandRunner.Step();
            } while (_hasSpawnedCommandsThisFrame);

            // TODO: We should probably dipose event StructValues holding strings at this point. But we might need to
            //       have a continuation task to the command, to be sure its not going to be used anymore.
        }

        IEnumerator<TaskContract> TriggerCommands(NativeDynamicArray commandList,
            StructValue eventValue, GUIComponent gui)
        {
            var commandCount = commandList.Count<CommandData>();
            for (var cIndex = 0; cIndex < commandCount; cIndex++)
            {
                var command = commandList.Get<CommandData>(cIndex);
                var commandInstance = _commandsManager.GetCommand(command.id);

            #if SVELTO_GUI_DEBUG
                var root = _resources.FromECS<string>(gui.root);
                var name = _resources.FromECS<string>(gui.name);
                Svelto.Console.Log($"Start command {commandInstance.GetType()} on {root}:{name}");
            #endif

                if (TryGetCommandTarget(command, gui, out EGID target) == false) break;

                var commandTask = commandInstance.Execute(entitiesDB, target, eventValue
                    , GetParameter(command.parameter1, _resources, _blackboard, gui, eventValue)
                    , GetParameter(command.parameter2, _resources, _blackboard, gui, eventValue)
                    , GetParameter(command.parameter3, _resources, _blackboard, gui, eventValue));

                _hasSpawnedCommandsThisFrame = true;
                yield return commandTask.Continue();

                var commandState = (GUICommand.State)commandTask.Current.ToInt();
                if (commandState == GUICommand.State.Failed)
                {
                #if SVELTO_GUI_DEBUG
                    Svelto.Console.Log($"Command {commandInstance.GetType()} failed on {root}:{name}");
                #endif
                    break;
                }
            }
        }

        bool TryGetCommandTarget(CommandData command, GUIComponent gui, out EGID targetEgid)
        {
            // Find command target if any.
            var  target     = _resources.FromECS<string>(command.target);
            targetEgid = default;
            if (!string.IsNullOrEmpty(target))
            {
                // The target field accepts a special format to allow selecting the widget
                // map to use for the target lookup. The format is "@x:" where is a single
                // character that might be replaced by:
                //   * g (Global): meaning the target will be searched with a fullname
                //                 inside the global widget map.
                //   * c (Container): container target will be searched as a relative name
                //                    inside the widget map pointed by GUIComponent.container
                var targetRef = EntityReference.Invalid;
                if (GUITags.ExtractTag(target, out char tag, out string tagTarget))
                {
                    switch (tag)
                    {
                        case 'g':
                        {
                            targetRef = _widgetMap.GetWidgetReference(tagTarget);
                            break;
                        }
                        case 'c':
                        {
                            var guiContainer = _resources.FromECS<string>(gui.container);
                            GUITags.ParseFullname(guiContainer, out var containerRoot, out var containerName);
                            var name = string.IsNullOrEmpty(tagTarget) ? containerName : tagTarget;
                            targetRef = _widgetMap.GetWidgetReference(containerRoot, name);
                            break;
                        }
                        case 's':
                        {
                            var root = _resources.FromECS<string>(gui.root);
                            var name = _resources.FromECS<string>(gui.name);
                            targetRef = _widgetMap.GetWidgetReference(root, name);
                            break;
                        }
                    }
                }
                else
                {
                    var guiRoot = _resources.FromECS<string>(gui.root);
                    targetRef = _widgetMap.GetWidgetReference(guiRoot, target);
                }

                if (targetRef.Equals(EntityReference.Invalid) == false)
                {
                    targetEgid = entitiesDB.GetEGID(targetRef);
                }
                else
                {
                    var guiRoot = _resources.FromECS<string>(gui.root);
                    var guiName = _resources.FromECS<string>(gui.name);
                    string errorMessage =
                        $"{guiRoot}.{guiName} Command target:{target} is not a valid widget in the GUI.";
                #if UNITY_EDITOR
                    errorMessage += "If this is a custom event consider that only global targets are supported";
                    Svelto.Console.LogWarning(errorMessage);
                #else
                    throw new ArgumentException(errorMessage);
                #endif

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Command parameters can hold different tags that are interpreted by the framework, if a tag is not found or
        /// there is no tagging at all the paremeter gets treated as a literal.
        /// Recognized tags are:
        ///   * "@d:" Data Key - parameter will be used as a key into the widget data source.
        ///   * "@i:" Indexed Data Key - parameter will be used in conjunction with the event value to query a key
        ///           inside an array.
        /// </summary>
        static string GetParameter(EcsResource parameterResource, GUIResources resources, GUIBlackboard blackboard,
            in GUIComponent gui, in StructValue eventValue)
        {
            var parameter = resources.FromECS<string>(parameterResource);
            if (GUITags.ExtractTag(parameter, out char tag, out parameter))
            {
                if (gui.root == EcsResource.Empty)
                {
                    string errorMessage = "SveltoGUI: custom events don't support tags for command parameters.";
                #if UNITY_EDITOR
                    Svelto.Console.LogError(errorMessage);
                    return parameter;
                #else
                    throw new ArgumentException(errorMessage);
                #endif
                }
                var widgetData = blackboard.GetData(gui);
                if (tag == 'd')
                {
                    // TODO: Check if we can solve this by returning struct values instead.
                    if (widgetData.TryGetValue(parameter, out object value))
                    {
                        return value.ToString();
                    }
                    else
                    {
#if UNITY_EDITOR
                        var widgetName = resources.FromECS<string>(gui.name);
                        Svelto.Console.LogWarning($"Widget binding value could not be found. Parameter: {parameter}, Widget name: {widgetName}");
#endif
                        return default;
                    }
                }

                if (tag == 'i')
                {
                    var splitPosition = parameter.IndexOf('.');
                    DBC.Check.Ensure(splitPosition > 0,
                        "Parameters tag with @i require the format 'keyToArray.keyToField` or 'keyToArray%i.keyToField");
                    var arrayKey = parameter.Substring(0, splitPosition);
                    var fieldKey = parameter.Substring(splitPosition + 1);

                    // Get index from event or from array key.
                    var index = eventValue.AsUint();
                    splitPosition = arrayKey.IndexOf('%');
                    if (splitPosition > 0)
                    {
                        string indexedKey = arrayKey;
                        arrayKey = indexedKey.Substring(0, splitPosition);
                        index = uint.Parse(indexedKey.Substring(splitPosition + 1));
                    }

                    var array = widgetData.GetArray<WidgetDataSource>(arrayKey);
                    return array[index].GetValue<string>(fieldKey);
                }
            }

            return parameter;
        }

        public void Dispose()
        {
            _commandRunner.Dispose();
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public bool isDisposing { get; set; }

        bool _hasSpawnedCommandsThisFrame;

        readonly CommandsManager _commandsManager;
        readonly GUIResources _resources;
        readonly GUIBlackboard _blackboard;
        readonly GUIWidgetMap _widgetMap;
        readonly SteppableRunner _commandRunner;
        readonly TriggeredEvents _triggeredEvents;
    }
}