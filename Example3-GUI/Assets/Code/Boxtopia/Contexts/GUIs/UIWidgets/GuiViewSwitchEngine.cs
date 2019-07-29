using System.Collections;
using Svelto.ECS;
using Svelto.Tasks.ExtraLean;

namespace Boxtopia.GUIs.Generic
{
    class GuiViewSwitchEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { private get; set; }

        public GuiViewSwitchEngine(IEntityStreamConsumerFactory consumerFactory)
        {
            _buttonConsumer = consumerFactory.GenerateConsumer<ButtonEntityStruct>(Boxtopia.GUIs.ExclusiveGroups.GuiViewButton, "MaterialEditorViewSwitchButtons", 1);
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
                        IGuiViewIndex guiViewIndex = entitiesDB.QueryEntity<GuiViewIndexEntityViewStruct>(entity.ID).guiViewIndex;

                        ExclusiveGroup.ExclusiveGroupStruct group = entity.ID.groupID;
                        if (string.IsNullOrEmpty(guiViewIndex.groupName) == false)
                            group = ExclusiveGroup.Search(guiViewIndex.groupName);

                        var entities = entitiesDB.QueryEntities<GUIEntityViewStruct>(group);
                            foreach (var gui in entities)
                                gui.guiRoot.view = guiViewIndex.index;
                    }
                }

                yield return null;
            }
        }

        readonly Consumer<ButtonEntityStruct> _buttonConsumer;
    }
}