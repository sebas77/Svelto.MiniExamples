using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.GUI.DBC;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    internal class GUIWidgetMap : IGUIWidgetMapWriter
    {
        internal GUIWidgetMap(GUIResources resources)
        {
            _resources = resources;
            _widgetsMap = new Dictionary<string, Dictionary<string, IWidgetDescriptorHolder>>();
            _widgetsByIdMap = new Dictionary<uint, IWidgetDescriptorHolder>();
            _widgetsByContainer = new Dictionary<string, FasterList<EntityReference>>();
        }

        public IEnumerable<IWidgetDescriptorHolder> GetWidgetMap(string rootName)
        {
            var found = _widgetsMap.TryGetValue(rootName, out var widgetMap);
            Check.Assert(found, $"Widget map not found for root:{rootName}");
            return widgetMap.Values;
        }

        public IEnumerable<IWidgetDescriptorHolder> GetWidgetMap(GUIComponent widgetComponent)
        {
            var rootName = _resources.FromECS<string>(widgetComponent.root);
            var found = _widgetsMap.TryGetValue(rootName, out var widgetMap);
            Check.Assert(found, $"Widget map not found for root:{rootName}");
            return widgetMap.Values;
        }

        public IWidgetDescriptorHolder GetWidget(GUIComponent widgetComponent)
        {
            return GetWidget(_resources.FromECS<string>(widgetComponent.root),
                            _resources.FromECS<string>(widgetComponent.name));
        }

        public IWidgetDescriptorHolder GetWidgetFromTag(GUIComponent gui, string identifier)
        {
            if (GUITags.ExtractTag(identifier, out char tag, out string target))
            {
                switch (tag)
                {
                    case 'g':
                        return GetWidget(target);
                    case 'c':
                        var containerName = _resources.FromECS<string>(gui.container);
                        GUITags.ParseFullname(containerName, out var containerRoot, out _);
                        return GetWidget(containerRoot, target);
                }

                throw new ArgumentException($"Unrecognized tag {tag}");
            }

            return GetWidget(_resources.FromECS<string>(gui.root), target);
        }

        public IWidgetDescriptorHolder GetWidget(string fullName)
        {
            GUITags.ParseFullname(fullName, out var root, out var name);
            return GetWidget(root, name);
        }

        public IWidgetDescriptorHolder GetWidget(string rootName, string widgetName)
        {
            return _widgetsMap[rootName][widgetName];
        }

        public IWidgetDescriptorHolder TryGetWidget(string fullName)
        {
            GUITags.ParseFullname(fullName, out var root, out var name);
            return TryGetWidget(root, name);
        }

        public IWidgetDescriptorHolder TryGetWidget(string rootName, string widgetName)
        {
            if (_widgetsMap.TryGetValue(rootName, out var widgetMap))
            {
                if (widgetMap.TryGetValue(widgetName, out IWidgetDescriptorHolder widget))
                {
                    return widget;
                }
            }

            return default;
        }

        public EntityReference GetWidgetReference(string fullName)
        {
            var widget = TryGetWidget(fullName);
            return widget?.entityReference ?? default;
        }

        public EntityReference GetWidgetReference(string rootName, string widgetName)
        {
            var widget = TryGetWidget(rootName, widgetName);
            return widget?.entityReference ?? default;
        }

        public EntityReference GetWidgetReference(uint id)
        {
            return _widgetsByIdMap[id].entityReference;
        }

        public void AddWidgetMap(string rootName, Dictionary<string, IWidgetDescriptorHolder> map)
        {
            _widgetsMap[rootName] = map;
        }

        public void RemoveWidgetMap(string rootName)
        {
            _widgetsMap.Remove(rootName);
        }

        public void SetWidgetUniqueId(IWidgetDescriptorHolder widgetHolder, IWidgetIDHolder idHolder)
        {
            DBC.Check.Require(widgetHolder != null);
            DBC.Check.Require(idHolder != null);

            idHolder.WidgetID = _nextWidgetUniqueId++;
            _widgetsByIdMap[idHolder.WidgetID] = widgetHolder;
        }

        public EntityReference[] GetContainerWidgets(string container)
        {
            if (_widgetsByContainer.TryGetValue(container, out var children))
            {
                return children.ToArray();
            }

            return Array.Empty<EntityReference>();
        }

        internal void AddToContainer(string container, EntityReference widget)
        {
            if (_widgetsByContainer.TryGetValue(container, out var containedWidgets) == false)
            {
                containedWidgets = new FasterList<EntityReference>();
                _widgetsByContainer[container] = containedWidgets;
            }

            containedWidgets.Add(widget);
        }

        internal void RemoveFromContainer(string container, EntityReference widget)
        {
            DBC.Check.Require(_widgetsByContainer.ContainsKey(container));
            var containedWidgets = _widgetsByContainer[container];
            for (var i = 0; i < containedWidgets.count; i++)
            {
                if (containedWidgets[i] == widget)
                {
                    containedWidgets.RemoveAt((uint)i);
                    break;
                }
            }
        }

        readonly Dictionary<string, Dictionary<string, IWidgetDescriptorHolder>> _widgetsMap;
        readonly Dictionary<uint, IWidgetDescriptorHolder> _widgetsByIdMap;
        readonly Dictionary<string, FasterList<EntityReference>> _widgetsByContainer;
        uint _nextWidgetUniqueId = 1;
        readonly GUIResources _resources;
    }
}