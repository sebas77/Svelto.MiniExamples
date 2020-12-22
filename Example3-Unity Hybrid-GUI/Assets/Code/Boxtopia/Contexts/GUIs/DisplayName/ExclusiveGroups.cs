using Svelto.ECS;

namespace Boxtopia.GUIs.DisplayName
{
    public static class ExclusiveGroups
    {
        public static readonly ExclusiveGroup DisplayName = new ExclusiveGroup();
        public static readonly ExclusiveGroup FeedbackLabel = new ExclusiveGroup("DisplayName.FeedbackLabel");
    }
}