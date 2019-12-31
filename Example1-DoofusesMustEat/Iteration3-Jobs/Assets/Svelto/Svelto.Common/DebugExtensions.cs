using System;
using System.Collections.Generic;

namespace Svelto.Common.Internal
{
    public static class DebugExtensions
    {
        public static string TypeName<T>(this T any)
        {
#if DEBUG && !PROFILE          
            var type = any.GetType();
            if (_names.TryGetValue(type, out var name) == false)
            {
                name = type.ToString();
                _names[type] = name;
            }

            return name;
#else
            return "";
#endif
        }
#if DEBUG && !PROFILE          
        static readonly Dictionary<Type, string> _names = new Dictionary<Type, string>();
#endif
    }
}