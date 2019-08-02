using ServiceLayer;
using Svelto.ECS;

namespace Boxtopia.GUIs.LocalisedText
{
    public class LocalizingTextEngine : IReactOnAddAndRemove<LocalizedLabelEntityViewStruct>, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {}

        public void Add(ref LocalizedLabelEntityViewStruct entityView)
        {
            entityView.label.text = LocalizationService.Localize(entityView.label.textKey);
        }

        public void Remove(ref LocalizedLabelEntityViewStruct entityView)
        {
        }
    }
}