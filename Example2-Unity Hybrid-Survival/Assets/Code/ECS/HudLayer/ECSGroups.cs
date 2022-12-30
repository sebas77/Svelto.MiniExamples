
namespace Svelto.ECS.Example.Survive.HUD
{
    /// <summary>
    ///     The list of Exclusive Groups used to build Entities.
    ///     the groups do not need to be put inside a single class. The user can decide to declare them as they wish.
    ///     The proper way to declare groups is to declare group in the same layer of abstraction of the engines that
    ///     need to use them. Using asmdefs and separate composition root would help to better define the boundaries
    ///     of the level of abstractions (which becomes a necessity on large projects). 
    ///     Groups may be not intuitive at first. Every ECS implementation has the concept of groups, although
    ///     often they are hidden. in Unity ECS there is a group for each archetype. Svelto doesn't have archetypes
    ///     Svelto has EntityDescriptors, so you can start from those.
    ///     Groups are a simple way to store subset of entities that are found in specific states.
    ///     ExclusiveGroup is the basic way to define a group, while GroupTag/GroupCompounds make them more flexible.
    ///     
    /// </summary>
    public static class ECSGroups
    {
        public static readonly ExclusiveGroup GUICanvas = new ExclusiveGroup();

        public static EGID   HUD = new EGID(0, GUICanvas);
        //Reserve a book range, as many groups as the possible number of player enemies
    }
}
