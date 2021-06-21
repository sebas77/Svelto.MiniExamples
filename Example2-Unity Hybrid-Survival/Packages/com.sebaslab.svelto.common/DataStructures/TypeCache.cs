using System;

namespace Svelto.Common
{
    public class TypeCache<T>
    {
        public static readonly Type type = typeof(T);
        public static readonly bool IsUnmanaged = type.IsUnmanagedEx();
    }

    public class TypeHash<T>
    {
#if !UNITY_BURST        
        public static readonly int hash = TypeCache<T>.type.GetHashCode();
#else
        public static readonly int hash = Unity.Burst.BurstRuntime.GetHashCode32<T>();
#endif        
    }
}