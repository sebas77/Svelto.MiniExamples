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
        
        //GroupCompounds enable Entities to be grouped by States and Adjectives. GroupCompounds are a simple way to handle Groups of entities separated not
        //only by their entity type but also by their current state. To switch state is enough to switch
        //group using the GroupCompound buildgroup. This is how Svelto fills the gap of the lack to dynamically Add/Remove components 
        //and GroupCompounds effectively replace component tags.
        //use group compounds as much as you can, it will make your code future proof. The reasoning about the group
        //compound should be as simple as this:
        //The first tag is what describes your entity descriptor
        //from the second tag on, the tags describe entities adjectives or states
        //BuildGroup are used for build/remove/swap
        //Groups for query/iterations
        //The goal of GroupCompound is to handle group management automatically for specialised and 
        //semi specialised engines.
        //You can add at any time more tags, but the groups used in code will never need to change
        //Note: using public class like this should be the pattern to use, as in this way, if you want to change
        //the meaning of a specific combination of states, you will need to change it just in one place.

        public class RED_DOOFUSES_EATING : GroupCompound<DOOFUSES, RED, EATING> { };
        public class RED_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, RED, NOTEATING> { };
        public class RED_FOOD_EATEN : GroupCompound<FOOD, RED, EATING> { };
        public class RED_FOOD_NOT_EATEN : GroupCompound<FOOD, RED, NOTEATING> { };
        
        public class BLUE_DOOFUSES_EATING : GroupCompound<DOOFUSES, BLUE, EATING> { };
        public class BLUE_DOOFUSES_NOT_EATING :  GroupCompound<DOOFUSES, BLUE, NOTEATING> { };
        public class BLUE_FOOD_EATEN : GroupCompound<FOOD, BLUE, EATING> { };
        public class BLUE_FOOD_NOT_EATEN : GroupCompound<FOOD, BLUE, NOTEATING> { };

        public class DOOFUSES_EATING : GroupCompound<DOOFUSES, EATING> { };
    }
}