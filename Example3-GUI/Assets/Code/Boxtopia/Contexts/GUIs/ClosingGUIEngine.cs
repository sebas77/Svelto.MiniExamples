using System.Collections;
using Boxtopia.GUIs.Generic;
using Svelto.ECS;
using Svelto.Tasks.ExtraLean;

namespace Boxtopia.GUIs
{
    public class ClosingGUIEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        public void        Ready()    { PollForButtonClicked().RunOn(ExtraLean.BoxtopiaSchedulers.UIScheduler); }

        public ClosingGUIEngine(IEntityStreamConsumerFactory generateConsumer)
        {
            _generateConsumer = generateConsumer;
        }

        IEnumerator PollForButtonClicked()
        {
            var generateConsumer = _generateConsumer.GenerateConsumer<ButtonEntityStruct>("ClosingGUIEngine", 1);
            
            while (true)
            {
                while (generateConsumer.TryDequeue(out var entity))
                {
                    if (entity.message == ButtonEvents.OK || entity.message == ButtonEvents.CANCEL)
                    {
                        // The buttons are contextual to the GUI that owns them, so the group must be the same
                        var guiEntityViewStructs =
                            entitiesDB.QueryEntities<GUIEntityViewStruct>(entity.ID.groupID, out var count);

                        for (int i = 0; i < count; i++)
                            guiEntityViewStructs[i].guiRoot.enabled = false;
                    }
                }

                yield return null;
            }
        }

        readonly IEntityStreamConsumerFactory _generateConsumer;
    }
}