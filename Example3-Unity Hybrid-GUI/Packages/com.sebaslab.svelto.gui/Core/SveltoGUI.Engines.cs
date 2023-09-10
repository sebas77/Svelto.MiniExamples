using Svelto.DataStructures;
using Svelto.ECS.GUI.Engines;

namespace Svelto.ECS.GUI
{
    partial class SveltoGUI
    {
        public void Tick()
        {
            for (var i = 0; i < _tickingEngines.count; i++)
            {
                _tickingEngines[i].Tick();
            }

            _scheduler?.SubmitEntities();
        }

        void InitEngines()
        {
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            // Add engines.
            _tickingEngines = new FasterList<IGUITickingEngine>();
            AddEngine(new GUIEventsEngine(_commandsManager, _resources, _widgetsMap, _blackboard, _triggeredEvents));
            AddEngine(new DynamicGUILifetimeEngine(_builder));
            AddEngine(new GUIWidgetLifetimeEngine(_widgetsMap, _resources, _blackboard, entityFunctions));
            AddEngine(new GUIFrameworkEventsDisposingEngine(_resources));
            AddEngine(new GUIWidgetEventsDisposingEngine(_resources));
            AddEngine(new GUIDataBindingEngine(_blackboard, _widgetsMap));
            AddEngine(new GUIDataEventEngine(_triggeredEvents));
            AddEngine(new GUIInitEventEngine(_triggeredEvents));
        }

        public void AddEngine(IEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            if (engine is IGUITickingEngine)
            {
                _tickingEngines.Add((IGUITickingEngine)engine);
            }
        }

        FasterList<IGUITickingEngine> _tickingEngines;
    }
}