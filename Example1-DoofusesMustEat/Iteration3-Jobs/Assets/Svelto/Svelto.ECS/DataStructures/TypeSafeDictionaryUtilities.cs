namespace Svelto.ECS.Internal
{
    static class TypeSafeDictionaryUtilities
    {
        internal static EGIDMapper<T> ToEGIDMapper<T>(this ITypeSafeDictionary<T> dic,
            ExclusiveGroupStruct groupStructId) where T:struct, IEntityStruct
        {
            var mapper = new EGIDMapper<T>(groupStructId, dic);

            return mapper;
        }

        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this TypeSafeDictionary<T> dic,
            ExclusiveGroupStruct groupStructId) where T : unmanaged, IEntityStruct
        {
            var mapper = new NativeEGIDMapper<T>(groupStructId, dic.implementation.ToNative<uint, T>());

            return mapper;
        }
    }
}