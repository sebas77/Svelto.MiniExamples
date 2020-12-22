using System.Collections;
using Svelto.ECS;
using Svelto.Tasks.ExtraLean;

namespace Boxtopia.GUIs.Generic
{
    class GuiViewSwitchEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { private get; set; }

        public GuiViewSwitchEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _buttonConsumer = consumerFactory.GenerateConsumer<ButtonEntityComponent>(ExclusiveGroups.GuiViewButton, "MaterialEditorViewSwitchButtons", 1);
        }

        public void Ready()
        {
            PollForInput().RunOn(ExtraLean.BoxtopiaSchedulers.UIScheduler);
        }

        IEnumerator PollForInput()
        {
            while (true)
            {
                while (_buttonConsumer.TryDequeue(out var entity))
                {
                    if (entity.message == ButtonEvents.SELECT)
                    {
                        IGuiViewIndex guiViewIndex = entitiesDB.QueryEntity<GuiViewIndexEntityViewComponent>(entity.ID).guiViewIndex;

                        ExclusiveGroupStruct group = entity.ID.groupID;
                        if (string.IsNullOrEmpty(guiViewIndex.groupName) == false)
                            group = ExclusiveGroup.Search(guiViewIndex.groupName);

                        var (entities, count) = entitiesDB.QueryEntities<GUIEntityViewComponent>(@group);
                            for (int i = 0; i < count; i++)
                                entities[i].guiRoot.view = guiViewIndex.index;
                    }
                }

                yield return null;
            }
        }

        readonly Consumer<ButtonEntityComponent> _buttonConsumer;
    }
}