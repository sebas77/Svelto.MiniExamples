using System.Collections.Generic;
using Svelto.ECS.GUI.Resources;
using Svelto.Tasks;

namespace Svelto.ECS.GUI.Commands
{
    public abstract class GUICommand
    {
        // TODO: params string causes allocation, maybe change to a parameter struct.
        protected internal abstract IEnumerator<TaskContract> Execute(EntitiesDB entitiesDB, EGID target, StructValue value, params string[] parameters);

        protected GUIResources resources => _resources;
        internal GUIResources _resources;

        protected IGUIWidgetMapReader widgetMap => _widgetMap;
        internal IGUIWidgetMapReader _widgetMap;

        internal GUIBlackboard _blackboard;

        // TODO: Receiving a WidgetDataSource causes unnecessary allocation.
        protected void SetGUIData(EGID toEgid, WidgetDataSource widgetData, EntitiesDB entitiesDB)
        {
            var guiComponent       = entitiesDB.QueryEntity<GUIComponent>(toEgid);
            _blackboard.AddData(_resources.FromECS<string>(guiComponent.root), widgetData);
        }

        protected void SetGUIData(string dataSourceKey, WidgetDataSource widgetData)
        {
            _blackboard.AddData(dataSourceKey, widgetData);
        }

        // NOTE: Commands can decide to return a success or failed value at the end of execution to provide more
        //       information to the framework to improve responsiveness.
        protected internal enum State
        {
            None = 0,
            Success,
            Failed
        }
    }
}