using System;
using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    ///     The list of Exclusive Groups used to build Entities.
    ///     WHEN I FIRST WROTE THIS EXAMPLE, GROUPCOMPUNDS DIDN'T EXIST. THIS IS STILL A VALID EXAMPLE, BUT
    ///     GROUPCOMPOUNDS SHOULD BE PREFERRED FOR THE STANDARD CASE.
    ///     the groups do not need to be put inside a single class. The user can decide to declare them as they wish.
    ///     The proper way to declare groups is to declare group in the same layer of abstraction of the engines that
    ///     need to use them. Using asmdefs and separate composition root would help to better define the boundaries
    ///     of the level of abstractions. 
    ///     Groups may be not intuitive at first. Every ECS implementation has the concept of groups, although
    ///     often they are hidden. in Unity ECS there is a group for each archetype. Svelto doesn't have archetypes
    ///     Svelto has EntityDescriptors, so you can start from those.
    ///     
    /// </summary>
    static class ECSGroups
    {
        public static readonly ExclusiveGroup GUICanvas = new ExclusiveGroup();
        public static readonly ExclusiveGroup Camera = new ExclusiveGroup();

        //Groups used to build/remove/swap
        public static readonly ExclusiveGroup PlayersGunsGroup = new ExclusiveGroup();
        public static readonly ExclusiveGroup PlayersGroup     = new ExclusiveGroup();
        
        public static readonly ExclusiveGroup EnemiesGroup     = new ExclusiveGroup();
        public static readonly ExclusiveGroup EnemiesDeadGroup = new ExclusiveGroup();

        //it's also possible to regroup groups. It's quite a flexible system
        public static readonly ExclusiveGroupStruct[] DamageableEntitiesGroups = {EnemiesGroup, PlayersGroup};

        public static EGID   HUD = new EGID(0, GUICanvas);
        //Reserve a book range, as many groups as the possible number of player enemies
        public static ExclusiveGroup EnemiesToRecycleGroups = new ExclusiveGroup((ushort) Enum.GetNames(typeof(PlayerTargetType)).Length);
        public static ExclusiveGroup EnemiesTargetGroup     = PlayersGroup;
        public static ExclusiveGroup PlayerTargetsGroup = EnemiesGroup;
    }
}