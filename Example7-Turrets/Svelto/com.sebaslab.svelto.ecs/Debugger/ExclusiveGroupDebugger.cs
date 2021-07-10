using Svelto.ECS;
using System;
using System.Collections.Generic;
using System.Reflection;

#if DEBUG
public static class ExclusiveGroupDebugger
{
    static ExclusiveGroupDebugger()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            try
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != null && type.IsClass && type.IsSealed
                     && type.IsAbstract) //this means only static classes
                    {
                        var fields = type.GetFields();
                        foreach (var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                var    group = (ExclusiveGroup) field.GetValue(null);
                                string name  = $"{type.FullName}.{field.Name} ({(uint) group})";
                                GroupMap.idToName[(uint) @group] = name;
                            }

                            if (field.IsStatic && typeof(ExclusiveGroupStruct).IsAssignableFrom(field.FieldType))
                            {
                                var group = (ExclusiveGroupStruct) field.GetValue(null);

                                string name = $"{type.FullName}.{field.Name} ({(uint) group})";
                                GroupMap.idToName[(uint) @group] = name;
                            }
                        }
                    }
                }
            }
            catch
            {
                Svelto.Console.LogDebugWarning(
                    "something went wrong while gathering group names on the assembly: ".FastConcat(assembly.FullName));
            }
        }
    }

    public static string ToName(this in ExclusiveGroupStruct group)
    {
        var idToName = GroupMap.idToName;
        if (idToName.TryGetValue((uint) @group, out var name) == false)
            name = $"<undefined:{((uint) group).ToString()}>";

        return name;
    }
}

public static class GroupMap
{
    static GroupMap() { GroupMap.idToName = new Dictionary<uint, string>(); }

    internal static readonly Dictionary<uint, string> idToName;
}
#else
public static class ExclusiveGroupDebugger
{
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        return ((uint)group).ToString();
    }
}
#endif