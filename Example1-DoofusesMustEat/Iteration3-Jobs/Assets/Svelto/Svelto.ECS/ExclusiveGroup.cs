using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using UnityEditor;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// still experimental alternative to ExclusiveGroup, use this like:
    /// use this like:
    /// public class TriggersGroup : ExclusiveGroup<TriggersGroup> {}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NamedExclusiveGroup<T>:ExclusiveGroup
    {
        public static ExclusiveGroup Group = new ExclusiveGroup();
        public static string         name  = typeof(T).FullName;

        public NamedExclusiveGroup() { }
        public NamedExclusiveGroup(string recognizeAs) : base(recognizeAs)  {}
        public NamedExclusiveGroup(ushort range) : base(range) {}
    }

    public abstract class GroupCompound<T> where T:GroupCompound<T>
    {
        public static ExclusiveGroup[] Groups = new ExclusiveGroup[0];
    }

    public static class GroupCompound<G1, G2> where G1 : GroupCompound<G1> where G2: GroupCompound<G2>
    {
        public static ExclusiveGroup[] Groups = new ExclusiveGroup[1];

        static GroupCompound()
        {
            Groups[0] = new ExclusiveGroup();
            var Group = Groups[0];
            
            TypeGroupCache<G1>.Add(Group);  TypeGroupCache<G2>.Add(Group);
            //GroupCompound<G2, G1>.Group = Group;
        }

        public static ExclusiveGroupStruct Group => Groups[0];
    }
    
    public static class GroupCompound<G1, G2, G3> where G1 : GroupCompound<G1> where G2: GroupCompound<G2> where G3 : GroupCompound<G3>
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

    static class TypeGroupCache<T> where T : GroupCompound<T>
    {
        public static void Add(ExclusiveGroup @group)
        {
                var type = typeof(GroupCompound<T>);
                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                FieldInfo field = fieldInfos[0];

                var _array = (ExclusiveGroup[]) field.GetValue(null);

            Array.Resize(ref _array, _array.Length + 1);

            _array[_array.Length - 1] = group;
            
            field.SetValue(null, _array);
        }
    }

    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static ExclusiveGroup MyExclusiveGroup1 = new ExclusiveGroup();
    ///
    ///     public static ExclusiveGroup[] GroupOfGroups = { MyExclusiveGroup1, ...}; //for each on this!
    /// }
    /// </summary>
    public class ExclusiveGroup
    {
        public const uint MaxNumberOfExclusiveGroups = 2 << 20; 
        
        public ExclusiveGroup()
        {
            _group = ExclusiveGroupStruct.Generate();
        }

        public ExclusiveGroup(string recognizeAs)
        {
            _group = ExclusiveGroupStruct.Generate();

            _knownGroups.Add(recognizeAs, _group);
        }

        public ExclusiveGroup(ushort range)
        {
            _group = new ExclusiveGroupStruct(range);
#if DEBUG
            _range = range;
#endif
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveGroup group)
        {
            return group._group;
        }

        public static explicit operator uint(ExclusiveGroup group)
        {
            return group._group;
        }

        public static ExclusiveGroupStruct operator+(ExclusiveGroup a, uint b)
        {
#if DEBUG
            if (a._range == 0)
                throw new ECSException($"Adding values to a not ranged ExclusiveGroup: {(uint)a}");
            if (b >= a._range)
                throw new ECSException($"Using out of range group: {(uint)a} + {b}");
#endif
            return a._group + b;
        }
        
        public static ExclusiveGroupStruct Search(string holderGroupName)
        {
            if (_knownGroups.ContainsKey(holderGroupName) == false)
                throw new Exception("Named Group Not Found ".FastConcat(holderGroupName));

            return _knownGroups[holderGroupName];
        }

        static readonly Dictionary<string, ExclusiveGroupStruct> _knownGroups = new Dictionary<string,
            ExclusiveGroupStruct>();

#if DEBUG
        readonly ushort _range;
#endif
        readonly ExclusiveGroupStruct _group;
    }
}

#if future
        public static void ConstructStaticGroups()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Assemblies or types aren't guaranteed to be returned in the same order,
            // and I couldn't find proof that `GetTypes()` returns them in fixed order either,
            // even for builds made with the exact same source code.
            // So will sort reflection results by name before constructing groups.
            var groupFields = new List<KeyValuePair<string, FieldInfo>>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = GetTypesSafe(assembly);

                foreach (Type type in types)
                {
                    if (type == null || !type.IsClass)
                    {
                        continue;
                    }

                    // Groups defined as static members in static classes
                    if (type.IsSealed && type.IsAbstract)
                    {
                        FieldInfo[] fields = type.GetFields();
                        foreach(var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                groupFields.Add(new KeyValuePair<string, FieldInfo>($"{type.FullName}.{field.Name}", field));
                            }
                        }
                    }
                    // Groups defined as classes
                    else if (type.BaseType != null
                             && type.BaseType.IsGenericType
                             && type.BaseType.GetGenericTypeDefinition() == typeof(ExclusiveGroup<>))
                    {
                        FieldInfo field = type.GetField("Group",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        groupFields.Add(new KeyValuePair<string, FieldInfo>(type.FullName, field));
                    }
                }
            }

            groupFields.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

            for (int i = 0; i < groupFields.Count; ++i)
            {
                groupFields[i].Value.GetValue(null);
#if DEBUG
                var group = (ExclusiveGroup) groupFields[i].Value.GetValue(null);
                groupNames[(uint) group] = groupFields[i].Key;
#endif
            }
        }

        static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                Type[] types = assembly.GetTypes();

                return types;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

#if DEBUG
        static string[] groupNames = new string[ExclusiveGroup.MaxNumberOfExclusiveGroups];
#endif
#endif