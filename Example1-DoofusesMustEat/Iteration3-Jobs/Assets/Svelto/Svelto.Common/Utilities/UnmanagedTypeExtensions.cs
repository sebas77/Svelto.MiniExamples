using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Svelto.Common
{
    public static class UnmanagedTypeExtensions
    {
        static readonly Dictionary<Type, bool> cachedTypes =
            new Dictionary<Type, bool>();

        public static bool IsUnmanaged<T>() { return typeof(T).IsUnmanaged(); }

        public static bool IsUnmanaged(this Type t)
        {
            var result = false;
            
            if (cachedTypes.ContainsKey(t))
                return cachedTypes[t];
            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else if (t.IsGenericType || !t.IsValueType)
                    result = false;
                else
                    result = t.GetFields(BindingFlags.Public | 
                                         BindingFlags.NonPublic | BindingFlags.Instance)
                              .All(x => IsUnmanaged(x.FieldType));
            cachedTypes.Add(t, result);
            return result;
        }
    }
}