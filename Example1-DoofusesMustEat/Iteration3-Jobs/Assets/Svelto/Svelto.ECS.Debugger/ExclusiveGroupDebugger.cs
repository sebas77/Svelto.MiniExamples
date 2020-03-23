using System;
using System.Collections.Generic;
using System.Reflection;

namespace Svelto.ECS.Debugger
{
    public static class EGID
    {
        static EGID()
        {
            _idToName = new Dictionary<uint, string>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != null && type.IsClass && type.IsSealed && type.IsAbstract) //this means only static classes
                    {
                        var fields = type.GetFields();
                        foreach(var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                string name  = $"{type.FullName}.{field.Name}";
                                var    group = (ExclusiveGroup)field.GetValue(null);
                                _idToName[(ExclusiveGroupStruct)group] = name;
                            }
                        }
                    }
                }
            }
        }
        
        public static string GetGroupNameFromId(uint id)
        {
            if (!_idToName.TryGetValue(id, out var name)) name = $"<undefined:{id}>";

            return name;
        }
        
        static readonly Dictionary<uint, string> _idToName;
    }
}
