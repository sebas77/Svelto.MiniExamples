using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs.LocalisedText
{
    public struct LocalizedLabelEntityViewStruct : IEntityViewStruct
    {
        public ILabelText label;
        public EGID ID { get; set; }
    }
}