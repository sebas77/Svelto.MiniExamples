namespace Svelto.ECS.Internal
{
    static class TypeSafeDictionaryUtilities
    {
        internal static EGIDMapper<T> ToEGIDMapper<T>(this ITypeSafeDictionary<T> dic) where T:struct, IEntityStruct
        {
            var mapper = new EGIDMapper<T> {map = dic};

            return mapper;
        }

        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this TypeSafeDictionary<T> dic) where T : unmanaged, IEntityStruct
        {
            var mapper = new NativeEGIDMapper<T> {map = dic.implementation.ToNative<uint, T>()};

            return mapper;
        }
        
        internal static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this FastTypeSafeDictionary<T> dic) where T : unmanaged, IEntityStruct
        {
            var mapper = new NativeEGIDMapper<T> {map = dic.implementation.ToNative<T>()};

            return mapper;
        }
    }
}