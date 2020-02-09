using System;
using System.Reflection;

namespace Svelto.ECS
{
    public static class GroupCompound<G1, G2, G3> where G1 : GroupTag<G1> where G2: GroupTag<G2> where G3 : GroupTag<G3>
    {
        public static ExclusiveGroup[] Groups = new ExclusiveGroup[1];

        static GroupCompound()
        {
            var Group = new ExclusiveGroup();
            Groups[0] = Group;
            
            TypeGroupCache<G1>.Add(Group);  TypeGroupCache<G2>.Add(Group); TypeGroupCache<G3>.Add(Group);
            //            TypeGroupCache<G1, G2>.Add(Group);  TypeGroupCache<G2, G1>.Add(Group);

            //          GroupCompound<G3, G1, G2>.Group = Group;
            //        GroupCompound<G2, G3, G1>.Group = Group;
            //      GroupCompound<G3, G2, G1>.Group = Group;
            ///    GroupCompound<G1, G3, G2>.Group = Group;
            // GroupCompound<G2, G1, G3>.Group = Group;
        }
    }

    public static class GroupCompound<G1, G2> where G1 : GroupTag<G1> where G2 : GroupTag<G2>
    {
        public static readonly ExclusiveGroup[] Groups;

        static GroupCompound()
        {
            if (GroupCompound<G2, G1>.Groups != null)
                Groups = GroupCompound<G2, G1>.Groups;
            else
            {
                //reserve immediately a group for this specialized compound
                Groups = new ExclusiveGroup[1];
                Groups[0] = new ExclusiveGroup();

                Svelto.Console.Log(typeof(G1).ToString().FastConcat("-", typeof(G2).ToString(), "-",
                                                                    Groups[0].ToString()));

                //every abstract group preemptively adds this group, it may or may not be empty in future
                TypeGroupCache<G1>.Add(Groups[0]);
                TypeGroupCache<G2>.Add(Groups[0]);
            }
        }

        public static InternalGroup BuildGroup => new InternalGroup(Groups[0]);
    }
    
    public abstract class GroupTag<T> where T:GroupTag<T>
    {
        public static readonly ExclusiveGroup[] Groups = new ExclusiveGroup[1];

        static GroupTag()
        {
            Groups[0] = new ExclusiveGroup();
        }
    }

    static class TypeGroupCache<T> where T : GroupTag<T>
    {
        static FieldInfo _field;

        public static void Add(ExclusiveGroup @group)
        {
            if (_field == null)
            {
                var type       = typeof(GroupTag<T>);
                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                _field = fieldInfos[0];
            }

            var _array = (ExclusiveGroup[]) _field.GetValue(null);
            
            for (int i = 0; i < _array.Length; ++i)
                if (_array[i] == group)
                    throw new Exception("temporary must be transformed in unit test");

            Array.Resize(ref _array, _array.Length + 1);

            _array[_array.Length - 1] = group;
            
            _field.SetValue(null, _array);
        }
    }

    public struct InternalGroup
    {
        readonly ExclusiveGroupStruct _group;
        internal InternalGroup(ExclusiveGroupStruct @group) { _group = group; }
        
        public static implicit operator ExclusiveGroupStruct(InternalGroup groupStruct)
        {
            return groupStruct._group;
        }
    }
}