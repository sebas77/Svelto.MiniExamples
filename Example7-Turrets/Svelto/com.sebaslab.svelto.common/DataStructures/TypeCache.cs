using System;

namespace Svelto.Common
{
    public class TypeCache<T>
    {
        public static readonly Type Type        = typeof(T);
        public static readonly bool IsUnmanaged = Type.IsUnmanagedEx();
    }

    public class TypeHash<T>
    {
#if !UNITY_BURST        
        public static readonly int hash = TypeCache<T>.Type.GetHashCode();
#else
        public static readonly int hash = Unity.Burst.BurstRuntime.GetHashCode32<T>();
#endif        
    }
}