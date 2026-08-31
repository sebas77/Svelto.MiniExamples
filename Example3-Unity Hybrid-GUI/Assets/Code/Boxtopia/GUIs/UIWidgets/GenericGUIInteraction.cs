using System.Collections;
using Boxtopia.GUIs.Generic;
using Svelto.ECS;
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
            Tick();
        }

        async void Tick()
        {
            using (var consumer =
                _generateConsumer.GenerateConsumer<ButtonEntityComponent>("StandardButtonActions"))
            {
                while (entitiesDB != null)
                {
                    while (consumer.TryDequeue(out var entity))
                    {
                        var entitiesDb = entitiesDB;
                        if (entity.message == ButtonEvents.WANNAQUIT)
                        {
                            await System.Threading.Tasks.Task.Yield();
                        }

                        if (entity.message == ButtonEvents.QUIT)
                        {
                            Svelto.Console.Log("Quitting now");

                            Application.Quit();

                            return;
                        }

                        if (entity.message == ButtonEvents.OK || entity.message == ButtonEvents.CANCEL)
                        {
                            // Buttons belong to the GUI entity group that owns their hierarchy.
                            var entityHierarchy =
                                entitiesDb.QueryEntity<EntityHierarchyComponent>(entity.ID);
                            CloseGUI(entitiesDb, entityHierarchy.parentGroup);
                        }
                    }

                    await System.Threading.Tasks.Task.Yield();
                }
            }
        }

        static void CloseGUI(EntitiesDB entitiesDb, ExclusiveGroupStruct group)
        {
            var (guiEntityViewComponents, count) = entitiesDb.QueryEntities<GUIEntityViewComponent>(group);

            for (int i = 0; i < count; i++)
                guiEntityViewComponents[i].guiRoot.enabled = false;
        }

        readonly IEntityStreamConsumerFactory _generateConsumer;
    }
}
