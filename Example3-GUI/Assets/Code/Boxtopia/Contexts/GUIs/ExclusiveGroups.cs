using Svelto.ECS;

namespace Boxtopia.GUIs
{
    static class ExclusiveGroups
    {
        public static readonly ExclusiveGroup GuiViewButton = new ExclusiveGroup("GuiViewButton");
    }
}