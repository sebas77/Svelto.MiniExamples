using System;
using System.Linq;
using System.Reflection;

namespace Svelto.Common.Internal
{
    static class UnmanagedTypeExtensions
    {
        //System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T> doesn't exist in dotnet
        //it's something just for mono!
        internal static bool IsUnmanagedEx(this Type t)
        {
            var result = false;
            
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

            
            return result;
        }
    }
}