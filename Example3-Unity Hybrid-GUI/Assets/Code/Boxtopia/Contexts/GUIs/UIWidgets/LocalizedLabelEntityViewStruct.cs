using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs.LocalisedText
{
    public struct LocalizedLabelEntityViewComponent : IEntityViewComponent
    {
        public ILabelText label;
        
        public EGID ID { get; set; }
    }
}