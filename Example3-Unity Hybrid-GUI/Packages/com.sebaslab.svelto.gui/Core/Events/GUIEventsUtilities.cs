using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.GUI.Resources;
using Svelto.ECS.GUI.Commands;

namespace Svelto.ECS.GUI
{
    public static class GUIEventsUtilities
    {
        const string CommandNameKey       = "CommandName";
        const string CommandTargetKey     = "Target";
        const string CommandParam1Key     = "Parameter1";
        const string CommandParam2Key     = "Parameter2";
        const string CommandParam3Key     = "Parameter3";

        public static GUIWidgetEventsComponent CreateEventComponent(uint eventCount)
        {
            var eventsComponent = new GUIWidgetEventsComponent();
            eventsComponent.map = new SharedSveltoDictionaryNative<int, NativeDynamicArray>(eventCount);
            return eventsComponent;
        }

        public static bool AddEventCommands(ref GUIWidgetEventsComponent widgetEventsMap, int eventId
            , GUIResources guiResources, CommandsManager commandsManager
            , WidgetDataSource data, string commandDataKey, SerializedCommandData[] staticCommands)
        {
            // Get commands defined on the dynamic data.
            NativeDynamicArray commandsList = default;
            if (data.TryGetArray(commandDataKey, out WidgetDataSource[] clickCommands))
            {
                var commandsCount = (uint) clickCommands.Length;
                commandsList = NativeDynamicArray.Alloc<CommandData>(commandsCount + (uint) staticCommands.Length);
                AddCommands(guiResources, commandsManager, commandsList, clickCommands);
            }
            else if (staticCommands.Length > 0)
            {
                commandsList = NativeDynamicArray.Alloc<CommandData>((uint)staticCommands.Length);
            }

            // If there are any commands for the click event create the GUI event entity.
            if (commandsList.isValid && commandsList.Count<CommandData>() > 0 || staticCommands.Length > 0)
            {
                // Add static commands if any.
                AddCommands(guiResources, commandsManager, commandsList, staticCommands);
                widgetEventsMap.map.Add(eventId, commandsList);

                return true;
            }

            return false;
        }

        static void AddCommands(GUIResources guiResources, CommandsManager commandsManager
            , NativeDynamicArray commandsList, WidgetDataSource[] commandParameters)
        {
            for (uint i = 0; i < commandParameters.Length; i++)
            {
                var commandData = commandParameters[i];
                var commandName = commandData.GetValue<string>(CommandNameKey);
                var commandId   = commandsManager.GetCommandId(commandName);

                commandData.TryGetValue(CommandTargetKey, out string target);

                commandData.TryGetValue(CommandParam1Key, out string parameter1);
                commandData.TryGetValue(CommandParam2Key, out string parameter2);
                commandData.TryGetValue(CommandParam3Key, out string parameter3);

                commandsList.Add(new CommandData
                {
                    id           = commandId
                    , target     = guiResources.ToECS(target)
                    , parameter1 = guiResources.ToECS(parameter1)
                    , parameter2 = guiResources.ToECS(parameter2)
                    , parameter3 = guiResources.ToECS(parameter3)
                });
            }
        }

        internal static void AddCommands(GUIResources guiResources, CommandsManager commandsManager
            , NativeDynamicArray commandList, SerializedCommandData[] commands)
        {
            for (uint i = 0; i < commands.Length; i++)
            {
                var commandData = commands[i];
                var commandId   = commandsManager.GetCommandId(commandData.commandName);

                commandList.Add(new CommandData
                {
                    id           = commandId
                    , target     = guiResources.ToECS(commandData.target)
                    , parameter1 = guiResources.ToECS(commandData.parameter1)
                    , parameter2 = guiResources.ToECS(commandData.parameter2)
                    , parameter3 = guiResources.ToECS(commandData.parameter3)
                });
            }
        }
    }
}