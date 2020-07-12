using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public abstract class GroupCompound<G1, G2, G3>
        where G1 : GroupTag<G1> where G2 : GroupTag<G2> where G3 : GroupTag<G3>
    {
        static readonly FasterList<ExclusiveGroupStruct> _Groups;
        
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupCompound()
        {
            if ((_Groups = GroupCompound<G3, G1, G2>._Groups) == null)
            if ((_Groups = GroupCompound<G2, G3, G1>._Groups) == null)
            if ((_Groups = GroupCompound<G3, G2, G1>._Groups) == null)
            if ((_Groups = GroupCompound<G1, G3, G2>._Groups) == null)
            if ((_Groups = GroupCompound<G2, G1, G3>._Groups) == null)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);

                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                    
            //    Console.LogDebug("<color=orange>".FastConcat(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "-").FastConcat(typeof(G3).ToString(), "- Initialized ", Groups[0].ToString()), "</color>"));
                    
                GroupCompound<G1, G2>.Add(Group); //<G1/G2> and <G2/G1> must share the same array
                GroupCompound<G1, G3>.Add(Group);
                GroupCompound<G2, G3>.Add(Group);
                    
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
                GroupTag<G3>.Add(Group);
            }
    //        else
           //    Console.LogDebug(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "-").FastConcat(typeof(G3).ToString(), "-", Groups[0].ToString()));
        }

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);
    }

    public abstract class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
    {
        static FasterList<ExclusiveGroupStruct> _Groups; 
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupCompound()
        {
            _Groups = GroupCompound<G2, G1>._Groups;
            
            if (_Groups == null)
            {
                _Groups = new FasterList<ExclusiveGroupStruct>(1);
                var Group = new ExclusiveGroup();
                _Groups.Add(Group);
                
            //    Console.LogDebug((typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "- initialized ", Groups[0].ToString())));

                //every abstract group preemptively adds this group, it may or may not be empty in future
                GroupTag<G1>.Add(Group);
                GroupTag<G2>.Add(Group);
            }
        //    else
              //  Console.LogDebug(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "-", Groups[0].ToString()));
        } 

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);

        public static void Add(ExclusiveGroupStruct @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");
            
            _Groups.Add(group);
            
            GroupCompound<G2, G1>._Groups = _Groups;
            
        //    Console.LogDebug(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "- Add ", group.ToString()));
        }
    }

    //A Group Tag holds initially just a group, itself. However the number of groups can grow with the number of
    //combinations of GroupTags including this one. This because a GroupTag is an adjective and different entities
    //can use the same adjective together with other ones. However since I need to be able to iterate over all the
    //groups with the same adjective, a group tag needs to hold all the groups sharing it.
    public abstract class GroupTag<T> where T : GroupTag<T>
    {
        static FasterList<ExclusiveGroupStruct> _Groups = new FasterList<ExclusiveGroupStruct>(1);
        
        public static FasterReadOnlyList<ExclusiveGroupStruct> Groups => new FasterReadOnlyList<ExclusiveGroupStruct>(_Groups);

        static GroupTag()
        {
            _Groups.Add(new ExclusiveGroup());
            
          //  Console.LogDebug("New Group Tag: ".FastConcat(typeof(T).ToString() + "- ID" + Groups[0].ToString()));
        }

        //Each time a new combination of group tags is found a new group is added.
        internal static void Add(ExclusiveGroup @group)
        {
            for (int i = 0; i < _Groups.count; ++i)
                if (_Groups[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            _Groups.Add(group);
            
       //     Console.LogDebug("Add Group in Compound: ".FastConcat(typeof(T).ToString().FastConcat("- Add ID ", group.ToString())));
        }

        public static ExclusiveGroupStruct BuildGroup => new ExclusiveGroupStruct(_Groups[0]);
    }
}
