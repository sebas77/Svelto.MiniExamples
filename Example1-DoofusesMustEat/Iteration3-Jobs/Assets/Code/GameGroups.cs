namespace Svelto.ECS.MiniExamples.Example1C
{
    static class GameGroups
    {
        public class DOOFUSES : GroupTag<DOOFUSES> { }
        public class RED : GroupTag<RED> { }
        public class BLUE : GroupTag<BLUE> { }
        public class FOOD : GroupTag<FOOD> { }
        
        //to explore:
        public class EATING : GroupTag<EATING> { }
        public class NOTEATING : GroupTag<NOTEATING> { }
    }
}