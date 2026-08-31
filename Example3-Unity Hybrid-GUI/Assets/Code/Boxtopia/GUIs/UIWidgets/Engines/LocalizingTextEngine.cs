using ServiceLayer;
using Svelto.ECS;

namespace Boxtopia.GUIs.LocalisedText
{
    public class LocalizingTextEngine : IReactOnAddAndRemoveEx<LocalizedLabelEntityViewComponent>, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {}

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<LocalizedLabelEntityViewComponent> entities,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = entities;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                buffer[i].label.text = LocalizationService.Localize(buffer[i].label.textKey);
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<LocalizedLabelEntityViewComponent> entities,
            ExclusiveGroupStruct groupID) { }
    }
}
