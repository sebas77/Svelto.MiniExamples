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

        public static bool IsUnmanagedEx<T>() { return typeof(T).IsUnmanagedEx(); }

        public static bool IsUnmanagedEx(this Type t)
        {
            var result = false;
            
            if (cachedTypes.ContainsKey(t))
                return cachedTypes[t];
            
            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                result = true;
            else
                if (t.IsValueType && t.IsGenericType)
                {
                    var areGenericTypesAllBlittable = t.GenericTypeArguments.All(x => IsUnmanagedEx(x));
                    if (areGenericTypesAllBlittable)
                        result = t.GetFields(BindingFlags.Public | 
                                             BindingFlags.NonPublic | BindingFlags.Instance)
                                  .All(x => IsUnmanagedEx(x.FieldType));
                    else
                        return false;
                }
                else
                if (t.IsValueType)
                    result = t.GetFields(BindingFlags.Public | 
                                         BindingFlags.NonPublic | BindingFlags.Instance)
                              .All(x => IsUnmanagedEx(x.FieldType));

            cachedTypes.Add(t, result);
            return result;
        }
    }
}