using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI.Engines
{
    class GUIDataBindingEngine : IReactOnAddAndRemove<GUIComponent>
    {
        public GUIDataBindingEngine(GUIBlackboard blackboard, IGUIWidgetMapReader widgetMap)
        {
            _blackboard = blackboard;
            _widgetMap = widgetMap;
        }

        public void Add(ref GUIComponent gui, EGID egid)
        {
            var widgetHolder = _widgetMap.GetWidget(gui);
            var implementors = widgetHolder.GetGUIImplementors();

            foreach (var implementor in implementors)
            {
                _blackboard.AddBindings(gui, implementor);
            }
        }

        public void Remove(ref GUIComponent gui, EGID egid)
        {
        }

        readonly GUIBlackboard _blackboard;
        readonly IGUIWidgetMapReader _widgetMap;
        readonly GUIResources _resources;
    }
}