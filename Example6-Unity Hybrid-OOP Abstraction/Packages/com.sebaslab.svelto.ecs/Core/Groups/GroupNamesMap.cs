using System.Collections.Generic;
using Svelto.ECS;

static class GroupNamesMap
{
    /// <summary>
    /// c# Static constructors are guaranteed to be thread safe
    /// The runtime guarantees that a static constructor is only called once. So even if a type is called by multiple threads at the same time,
    /// the static constructor is always executed one time. To get a better understanding how this works, it helps to know what purpose it serves.
    /// </summary>
#if DEBUG
    static GroupNamesMap() { idToName = new Dictionary<ExclusiveGroupStruct, string>(); }

    internal static readonly Dictionary<ExclusiveGroupStruct, string> idToName;
#endif
#if DEBUG
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        Dictionary<ExclusiveGroupStruct, string> idToName = GroupNamesMap.idToName;
        if (idToName.TryGetValue(@group, out var name) == false)
            name = $"<undefined:{(group.id).ToString()}>";

        return name;
    }
#else
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        return ((uint)group.ToIDAndBitmask()).ToString();
    }
#endif
}