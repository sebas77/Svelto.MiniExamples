using System.Threading.Tasks;
using Boxtopia.GUIs.Generic;
using Svelto.ECS;

namespace Boxtopia.GUIs
{
    public class ClosingGUIEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        public void        Ready()    { Tick(); }

        public ClosingGUIEngine(IEntityStreamConsumerFactory generateConsumer)
        {
            _generateConsumer = generateConsumer;
        }

        async void Tick()
        {
            var consumer = _generateConsumer.GenerateConsumer<ButtonEntityComponent>("ClosingGUIEngine");

            while (entitiesDB != null)
            {
                ProcessButtonMessages(consumer);

                await Task.Yield();
            }
        }

        void ProcessButtonMessages(Consumer<ButtonEntityComponent> consumer)
        {
            while (consumer.TryDequeue(out var entity))
            {
                if (entity.message == ButtonEvents.OK || entity.message == ButtonEvents.CANCEL)
                {
                    // The buttons are contextual to the GUI that owns them, so the group must be the same
                    var (guiEntityViewComponents, count) = entitiesDB.QueryEntities<GUIEntityViewComponent>(entity.ID.groupID);

                    for (int i = 0; i < count; i++)
                        guiEntityViewComponents[i].guiRoot.enabled = false;
                }
            }
        }

        readonly IEntityStreamConsumerFactory _generateConsumer;
    }
}
