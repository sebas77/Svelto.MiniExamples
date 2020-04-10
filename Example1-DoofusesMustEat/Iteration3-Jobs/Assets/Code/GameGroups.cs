namespace Svelto.ECS.MiniExamples.Example1C
{
    static class GameGroups
    {
        public class DOOFUSES : GroupTag<DOOFUSES> { }
        public class FOOD : GroupTag<FOOD> { }
        
        public class RED : GroupTag<RED> { }
        public class BLUE : GroupTag<BLUE> { }
        
        public class EATING : GroupTag<EATING> { }
        public class NOTEATING : GroupTag<NOTEATING> { }
    }
}