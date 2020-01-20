namespace Svelto.ECS.MiniExamples.Example1B
{
    static class GameGroups
    {
        public class DOOFUSES : GroupCompound<DOOFUSES> { }
        public class RED : GroupCompound<RED> { }
        public class BLUE : GroupCompound<BLUE> { }

        public static readonly ExclusiveGroup FOOD = new ExclusiveGroup();
    }
}