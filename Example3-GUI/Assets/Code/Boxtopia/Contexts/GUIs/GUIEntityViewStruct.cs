using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs
{
    public struct GUIEntityViewStruct : IEntityViewStruct
    {
        public IGUIRoot guiRoot;
        public EGID     ID { get; set; }
    }
}