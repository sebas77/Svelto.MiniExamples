using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs
{
    public struct GUIEntityViewComponent : IEntityViewComponent
    {
        public IGUIRoot guiRoot;
        public EGID     ID { get; set; }
    }
}