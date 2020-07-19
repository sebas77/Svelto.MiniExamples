using ServiceLayer;
using Svelto.ECS;

namespace Boxtopia.GUIs.LocalisedText
{
    public class LocalizingTextEngine : IReactOnAddAndRemove<LocalizedLabelEntityViewComponent>, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {}

        public void Add(ref LocalizedLabelEntityViewComponent entityView, EGID egid)
        {
            entityView.label.text = LocalizationService.Localize(entityView.label.textKey);
        }

        public void Remove(ref LocalizedLabelEntityViewComponent entityView, EGID egid)
        {}
    }
}