using Svelto.ECS;

namespace Boxtopia.GUIs.NameValidation
{
    public static class ExclusiveGroups
    {
        public static readonly ExclusiveGroup NameValidation = new ExclusiveGroup();
        public static readonly ExclusiveGroup FeedbackLabel = new ExclusiveGroup("NameValidation.FeedbackLabel");
    }
}
