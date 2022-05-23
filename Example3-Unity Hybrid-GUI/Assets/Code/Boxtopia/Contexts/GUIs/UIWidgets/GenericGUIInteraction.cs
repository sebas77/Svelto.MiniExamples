using System.Collections;
using Boxtopia.GUIs.Generic;
using Svelto.ECS;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using UnityEngine;

namespace Boxtopia.GUIs
{
    public class GenericGUIInteraction : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        
        public GenericGUIInteraction(IEntityStreamConsumerFactory generateConsumer)
        {
            _generateConsumer = generateConsumer;
        }
        
        public void Ready()
        {
            PollForButtonClicked().RunOn(ExtraLean.BoxtopiaSchedulers.UIScheduler);
        }
        
        IEnumerator PollForButtonClicked()
        {
            using (var consumer =
                _generateConsumer.GenerateConsumer<ButtonEntityComponent>("StandardButtonActions", 1))
            {
                while (true)
                {
                    while (consumer.TryDequeue(out var entity))
                    {
                        var entitiesDb = entitiesDB;
                        if (entity.message == ButtonEvents.WANNAQUIT)
                        {
                            yield return Yield.It;
                        }
                        
                        if (entity.message == ButtonEvents.QUIT)
                        {
                            Svelto.Console.Log("Quitting now");

                            Application.Quit();

                            yield break;
                        }
                        
                        if (entity.message == ButtonEvents.OK || entity.message == ButtonEvents.CANCEL)
                        {// The buttons are contextual to the GUI that owns them, so the group must be the same
                            var entityHierarchy =
                                entitiesDb.QueryEntity<EntityHierarchyComponent>(entity.ID);
                            
                            var (guiEntityViewComponents, count) = entitiesDb.QueryEntities<GUIEntityViewComponent>(entityHierarchy.parentGroup);

                            for (int i = 0; i < count; i++)
                                guiEntityViewComponents[i].guiRoot.enabled = false;
                        }
                    }

                    yield return null;
                }
            }
        }

        readonly IEntityStreamConsumerFactory _generateConsumer;
    }
}