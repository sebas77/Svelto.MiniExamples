namespace Svelto.ECS.MiniExamples.Example1
{
    static class GameGroups
    {
        public static readonly ExclusiveGroup DOOFUSESHUNGRY = new ExclusiveGroup();
        public static readonly ExclusiveGroup DOOFUSESEATING = new ExclusiveGroup();
        public static readonly ExclusiveGroup FOOD = new ExclusiveGroup();
    }
}