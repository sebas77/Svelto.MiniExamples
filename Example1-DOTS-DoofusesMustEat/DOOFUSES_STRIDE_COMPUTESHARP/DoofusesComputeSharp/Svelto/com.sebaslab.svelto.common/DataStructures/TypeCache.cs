using System;

namespace Svelto.Common
{
    public static class TypeCache<T>
    {
        public static readonly Type   type = typeof(T);
        public static readonly string name = type.Name;
    }

    public static class TypeType
    {
        public static bool isUnmanaged<T>()
        {
#if !UNITY_BURST
            return TypeCache<T>.type.IsUnmanagedEx();
#else
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.IsUnmanaged<T>();
#endif
        }
    }

    public static class TypeHash<T>
    {
#if !UNITY_BURST
        public static readonly int hash = TypeCache<T>.type.GetHashCode();
#else
        public static readonly int hash = Unity.Burst.BurstRuntime.GetHashCode32<T>();
#endif
    }
}