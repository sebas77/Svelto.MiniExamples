using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI.Engines
{
    /**
     * This Engine is in charge of disposing all necessary ECS resources and data structures and child widgets.
     */
    class GUIWidgetLifetimeEngine : IQueryingEntitiesEngine, IReactOnAddAndRemove<GUIComponent>
    {
        public GUIWidgetLifetimeEngine(GUIWidgetMap widgetsMap, GUIResources resources, GUIBlackboard blackboard
            , IEntityFunctions functions)
        {
            _widgetsMap = widgetsMap;
            _resources  = resources;
            _blackboard = blackboard;
            _functions  = functions;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Add(ref GUIComponent gui, EGID egid)
        {
            // Store container widgets.
            if (gui.isRoot)
            {
                var container = _resources.FromECS<string>(gui.container);
                if (string.IsNullOrEmpty(container) == false)
                {
                    _widgetsMap.AddToContainer(container, egid.ToEntityReference(entitiesDB));
                }
            }
        }

        public void Remove(ref GUIComponent gui, EGID egid)
        {
            if (gui.isRoot)
            {
                // Remove all entities belonging to this widget map.
                var rootName  = _resources.FromECS<string>(gui.root);
                var rootWidget = _widgetsMap.GetWidget(gui);
                var widgetMap = _widgetsMap.GetWidgetMap(rootName);
                foreach (var widgetHolder in widgetMap)
                {
                    // Root widget is included in the widget map and we don't want to destroy it twice
                    if (rootWidget != widgetHolder)
                    {
                        _functions.RemoveEntity<GUIExtendibleEntityDescriptor.GuiEntityDescriptor>
                            (widgetHolder.entityReference.ToEGID(entitiesDB));
                    }

                    // Remove bindings
                    foreach (var guiImplementor in widgetHolder.GetGUIImplementors())
                    {
                        _blackboard.RemoveBindings(guiImplementor);
                    }
                }

                // Remove root widget data and map.
                _blackboard.RemoveData(gui);
                _widgetsMap.RemoveWidgetMap(rootName);

                // Remove from containers.
                var container = _resources.FromECS<string>(gui.container);
                if (string.IsNullOrEmpty(container) == false)
                {
                    _widgetsMap.RemoveFromContainer(container, rootWidget.entityReference);
                }

                // Empty containers.
                var containedWidgets = _widgetsMap.GetContainerWidgets(rootName);
                foreach (var widget in containedWidgets)
                {
                    _functions.RemoveEntity<GUIExtendibleEntityDescriptor.GuiEntityDescriptor>
                        (widget.ToEGID(entitiesDB));
                }

                // These resources are the same for all the widgets in the hierarchy so we can remove them just once on
                // the root widget.
                _resources.Release(gui.root);
                _resources.Release(gui.container);
            }
            // Release widget name.
            _resources.Release(gui.name);
        }

        readonly IEntityFunctions _functions;
        readonly GUIResources     _resources;
        readonly GUIBlackboard    _blackboard;
        readonly GUIWidgetMap     _widgetsMap;
    }
}