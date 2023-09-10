using Svelto.ECS.GUI.Commands;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI.Engines
{
    /**
     * This Engine is in charge of disposing all of the ECS resources and data structures linked to event widgets.
     */
    class GUIWidgetEventsDisposingEngine : IReactOnAddAndRemove<GUIWidgetEventsComponent>
    {
        public GUIWidgetEventsDisposingEngine(GUIResources guiResources)
        {
            _guiResources = guiResources;
        }

        public void Add(ref GUIWidgetEventsComponent widgetEventsComponent, EGID egid) { }

        public void Remove(ref GUIWidgetEventsComponent widgetEventsComponent, EGID egid)
        {
            if (widgetEventsComponent.map.isValid)
            {
                var eventCommands = widgetEventsComponent.map.GetValues(out var eCount);
                for (var eIndex = 0; eIndex < eCount; eIndex++)
                {
                    var commands = eventCommands[eIndex];
                    DBC.Check.Assert(commands.isValid);

                    var cCount = commands.Count<CommandData>();
                    for (var cIndex = 0; cIndex < cCount; cIndex++)
                    {
                        var command = commands.Get<CommandData>(cIndex);
                        _guiResources.Release(command.parameter1);
                        _guiResources.Release(command.parameter2);
                    }
                    commands.Dispose();
                }

                widgetEventsComponent.map.Dispose();
            }
        }

        readonly GUIResources _guiResources;
    }
}