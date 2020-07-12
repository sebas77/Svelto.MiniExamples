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
        
        
        //GroupCompounds enable Entities ///     to be grouped by States and Adjectives. GroupCompounds are a simple way to handle Groups of entities separated not
        ///     only by their entity type but also by their current state. To switch state is enough to switch
        ///     group using the GroupCompound buildgroup. This is how Svelto fills the gap of the lack of dynamic at run time Add/Remove components
        ///     and GroupCompounds effectively replace component tags.
        //use group compound as much as you can, it will make your code future proof. The reasoning about the group
        //compound should be as simple as this:
        //The first tag is what describes your entity descriptor
        //from the second tag on, the tags describe entities adjectives or states
        //BuildGroup are used for build/remove/swap
        //Groups for query/iterations
        //The goal of GroupCompound is to handle group management automatically for specialised and 
        //semi specialised engines.
        //You can add at any time more tags, but the groups used in code will never need to change
    //    class PlayerGroupCompound : GroupCompound<PLAYER, ALIVE> { };
  //      class PlayerWeaponsGroupCompound : GroupCompound<WEAPON, PLAYER> { };
//        class EnemiesGroupCompound : GroupCompound<ENEMY, ALIVE> { };
    }
}