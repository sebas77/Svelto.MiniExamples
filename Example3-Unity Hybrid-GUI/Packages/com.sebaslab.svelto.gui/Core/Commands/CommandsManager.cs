using System;
using System.Collections.Generic;
using System.Linq;
using Svelto.ECS.GUI.DBC;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI.Commands
{
    public class CommandsManager
    {
        internal CommandsManager(GUIBlackboard blackboard, GUIResources resources, GUIWidgetMap widgetMap)
        {
            _blackboard = blackboard;
            _resources = resources;
            _widgetMap = widgetMap;
            _commandId          = 0;
            _commandNameMap     = new Dictionary<string, int>();
            _commandMap         = new Dictionary<int, GUICommand>();

            foreach (var kvp in _assemblyCommands)
            {
                if (kvp.Value.autoRegister)
                {
                    RegisterCommand(kvp.Value.commandName, (GUICommand)Activator.CreateInstance(kvp.Key));
                }
            }
        }

        public int GetCommandId(string command)
        {
            var commandFound = _commandNameMap.TryGetValue(command, out var id);
            Check.Ensure(commandFound, $"Command {command} not found, it needs to be registered first.");
            return id;
        }

        public void RegisterCommand(GUICommand command)
        {
            var commandType = command.GetType();
            if (_assemblyCommands.TryGetValue(commandType, out var commandAttribute))
            {
                RegisterCommand(commandAttribute.commandName, command);
            }
            else
            {
                throw new Exception($"Error registering {commandType.Name}. Only classes implementing IGUICommand and decorated with the GUICommand attribute may be registered");
            }
        }

        void RegisterCommand(string commandName, GUICommand command)
        {
            command._blackboard = _blackboard;
            command._resources = _resources;
            command._widgetMap = _widgetMap;
            _commandNameMap.Add(commandName, ++_commandId);
            _commandMap.Add(_commandId, command);
        }

        internal GUICommand GetCommand(int i)
        {
            return _commandMap[i];
        }

        static CommandsManager()
        {
            // We use reflection to find commands that can be registered automatically.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var commands = assemblies.AsParallel().SelectMany(a => a.GetTypes())
                .Where(t => typeof(GUICommand).IsAssignableFrom(t))
                .Select(t => new Tuple<Type, GUICommandAttribute>(t, (GUICommandAttribute)Attribute.GetCustomAttribute(t, typeof(GUICommandAttribute))))
                .Where(tuple => tuple.Item2 != null).ToArray();

            _assemblyCommands = new Dictionary<Type, GUICommandAttribute>();
            foreach (var (type, attribute) in commands)
            {
                if (attribute.autoRegister && type.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new Exception($"SveltoGUI: Command {type.Name} has a GUICommandAttribute with autoRegister set to true but no parameterless constructor.");
                }

                _assemblyCommands[type] = attribute;
            }
        }

        int _commandId;

        readonly Dictionary<string, int>     _commandNameMap;
        readonly Dictionary<int, GUICommand> _commandMap;

        readonly        GUIBlackboard _blackboard;
        readonly        GUIResources  _resources;
        readonly        GUIWidgetMap  _widgetMap;

        static readonly Dictionary<Type, GUICommandAttribute> _assemblyCommands;
    }
}