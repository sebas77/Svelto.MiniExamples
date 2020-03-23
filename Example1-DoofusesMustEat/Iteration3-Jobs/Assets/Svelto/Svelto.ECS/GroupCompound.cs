using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class GroupCompound<G1, G2, G3>
        where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3>
    {
        public static ExclusiveGroup[] Groups = new ExclusiveGroup[1];

        static GroupCompound()
        {
            var Group = new ExclusiveGroup();
            Groups[0] = Group;

            GroupTag<G1>.Add(Group);
            GroupTag<G2>.Add(Group);
            GroupTag<G3>.Add(Group);

            GroupCompound<G1, G2>.Add(Group);
            GroupCompound<G2, G1>.Add(Group);

            GroupCompound<G3, G1, G2>.Groups = Groups;
            GroupCompound<G2, G3, G1>.Groups = Groups;
            GroupCompound<G3, G2, G1>.Groups = Groups;
            GroupCompound<G1, G3, G2>.Groups = Groups;
            GroupCompound<G2, G1, G3>.Groups = Groups;
        }

        public static InternalGroup BuildGroup => new InternalGroup(Groups[0]);
    }

    public static class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
    {
        public static ExclusiveGroup[] Groups;

        static GroupCompound()
        {
            //reserve immediately a group for this specialized compound
            Groups = new ExclusiveGroup[1];
            Groups[0] = new ExclusiveGroup();

            Console.Log(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "-",
                Groups[0].ToString()));

            //every abstract group preemptively adds this group, it may or may not be empty in future
            GroupTag<G1>.Add(Groups[0]);
            GroupTag<G2>.Add(Groups[0]);

            GroupCompound<G2, G1>.Groups = Groups;
        }

        public static InternalGroup BuildGroup => new InternalGroup(Groups[0]);

        public static void Add(ExclusiveGroup @group)
        {
            for (int i = 0; i < Groups.Length; ++i)
                if (Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            Array.Resize(ref Groups, Groups.Length + 1);

            Groups[Groups.Length - 1] = group;
        }
    }

    //A Group Tag holds initially just a group, itself. However the number of groups can grow with the number of
    //combinations of GroupTags including this one. This because a GroupTag is an adjective and different entities
    //can use the same adjective together with other ones. However since I need to be able to iterate over all the
    //groups with the same adjective, a group tag needs to hold all the groups sharing it.
    public abstract class GroupTag<T> where T : GroupTag<T>
    {
        public static ExclusiveGroup[] Groups = new ExclusiveGroup[1];

        static GroupTag()
        {
            Groups[0] = new ExclusiveGroup();
        }

        //Each time a new combination of group tags is found a new group is added.
        internal static void Add(ExclusiveGroup @group)
        {
            for (int i = 0; i < Groups.Length; ++i)
                if (Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            Array.Resize(ref Groups, Groups.Length + 1);

            Groups[Groups.Length - 1] = group;
        }
    }
}