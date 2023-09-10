using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.GUI.Commands;

namespace Svelto.ECS.GUI
{
    public partial class SveltoGUI
    {
        public EntityReference BuildDynamicWidget(string widgetName, string containerFullname
            , WidgetDataSource dataSource)
        {
            if (containerFullname == null)
            {
                Svelto.Console.LogWarning("Creating dynamic widgets without a container is no longer possible.");
                return default;
            }

            if (widgetName == null)
            {
                Svelto.Console.LogWarning("Creating dynamic widget without a resource name.");
                return default;
            }

            var container = _widgetsMap.GetWidget(containerFullname);
            _builder.CreateInstance(widgetName, container, out var widget, out var recyclable);

            var initializer = BuildFullWidget(widget, dataSource, containerFullname, true);
            initializer.Init(recyclable);

            return initializer.reference;
        }

        void StaticBuild()
        {
            var widgets = _builder.GetStaticWidgets();

            foreach (var widget in widgets)
            {
                BuildFullWidget(widget, new WidgetDataSource());
            }

            _scheduler?.SubmitEntities();
        }

        EntityInitializer BuildFullWidget(WidgetDefinition widgetDefinition, WidgetDataSource dataSource
            , string container = null, bool dynamic = false)
        {
            var rootWidget = widgetDefinition.root;
            if (BuildWidget(rootWidget, dataSource, out var builder, out var rootInitializer) == false)
            {
                return rootInitializer;
            }

            // Setup GUI component names.
            string rootName = rootWidget.GetName();
            bool hasName = string.IsNullOrEmpty(rootName) == false;
            rootName = hasName ? rootName : rootInitializer.reference.ToString();

            var rootInstanceName = rootName;
            if (dynamic && hasName)
            {
                rootInstanceName = $"{rootName}:{rootInitializer.reference.ToString()}";
            }

            var ecsRootInstanceName = _resources.ToECS(rootInstanceName);
            var ecsRootName         = _resources.ToECS(rootName);
            var ecsContainerName    = _resources.ToECS(container);

            var guiComponent = new GUIComponent
            {
                isRoot    = true,
                root      = ecsRootInstanceName,
                name      = ecsRootName,
                container = ecsContainerName
            };
            rootInitializer.Init(guiComponent);

            // Add data to the blackboard.
            _blackboard.AddData(rootInstanceName, dataSource);

            // Add to widget map.
            var map = new Dictionary<string, IWidgetDescriptorHolder>();
            map.Add(rootName, rootWidget);
            _widgetsMap.AddWidgetMap(rootInstanceName, map);

            // TODO: Once we are using bindings for everything we won't need this anymore. Even if events don't get
            //       bindings we could just setup events directly on the builder.
            // Initialize widget.
            builder.Initialize(rootInitializer, rootWidget, dataSource);

            guiComponent.isRoot = false;
            foreach (var childWidget in widgetDefinition.childWidgets)
            {
                if (BuildWidget(childWidget, dataSource, out builder, out var childInitializer) == false)
                {
                    continue;
                }

                // Set gui component.
                var childName = childWidget.GetName();
                childName = !string.IsNullOrEmpty(childName) ? childName : childWidget.entityReference.ToString();
                guiComponent.name = _resources.ToECS(childName);
                childInitializer.Init(guiComponent);

                // Add to map.
                map.Add(childName, childWidget);

                builder.Initialize(childInitializer, childWidget, dataSource);
            }

            return rootInitializer;
        }

        bool BuildWidget(IWidgetDescriptorHolder holder, WidgetDataSource dataSource, out IWidgetBuilder builder
            , out EntityInitializer initializer)
        {
            var widgetType = holder.GetType();
            if (_widgetBuilders.TryGet(widgetType, out builder) == false)
            {
                Svelto.Console.LogWarning($"No gui builder found for type {widgetType}");
                initializer = default;
                return false;
            }

            // TODO: This is were we want to replace GetImplementors to actually GetGUIImplementors to enforce the usage
            //       of bindings.
            // Go through the builder to actually build the entity. This is user code, because the user should decide
            // group and entity id. This can depend on the data source.
            initializer = builder.BuildWidget(holder, holder.GetImplementors(), dataSource);
            holder.entityReference = initializer.reference;

            // Add framework level events.
            var frameworkEvents = new GUIFrameworkEventsComponent();
            frameworkEvents.map = new SharedSveltoDictionaryNative<int, NativeDynamicArray>((uint)FrameworkEvents.Count);

            var dataCommands = holder.GetDataEventCommands(dataSource);
            if (dataCommands.Length > 0)
            {
                var commandList = NativeDynamicArray.Alloc<CommandData>(Allocator.Persistent, (uint)dataCommands.Length);
                GUIEventsUtilities.AddCommands(_resources, _commandsManager, commandList, dataCommands);
                frameworkEvents.map[(int)FrameworkEvents.Data] = commandList;
            }

            var initCommands = new List<SerializedCommandData>(dataCommands);
            initCommands.AddRange(holder.GetInitEventCommands(dataSource));
            if (initCommands.Count > 0)
            {
                var commandList = NativeDynamicArray.Alloc<CommandData>(Allocator.Persistent, (uint)initCommands.Count);
                GUIEventsUtilities.AddCommands(_resources, _commandsManager, commandList, initCommands.ToArray());
                frameworkEvents.map[(int)FrameworkEvents.Init] = commandList;
            }

            initializer.Init(frameworkEvents);

            return true;
        }
    }
}