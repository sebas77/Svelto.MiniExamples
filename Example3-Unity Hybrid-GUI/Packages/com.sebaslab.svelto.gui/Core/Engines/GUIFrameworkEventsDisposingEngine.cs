using Svelto.ECS.GUI.Commands;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI.Engines
{
    /**
     * This Engine is in charge of disposing all of the ECS resources and data structures linked to event widgets.
     */
    class GUIFrameworkEventsDisposingEngine : IReactOnRemove<GUIFrameworkEventsComponent>
    {
        public GUIFrameworkEventsDisposingEngine(GUIResources guiResources)
        {
            _guiResources = guiResources;
        }

        public void Remove(ref GUIFrameworkEventsComponent eventsComponent, EGID egid)
        {
            if (eventsComponent.map.isValid)
            {
                var eventCommands = eventsComponent.map.GetValues(out var eCount);
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

                eventsComponent.map.Dispose();
            }
        }

        readonly GUIResources _guiResources;
    }
}