using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class TypeSafeDictionaryFactory<T> where T : struct, IBaseEntityComponent
    {
        public static ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<T>(1);
        }
        
        public static ITypeSafeDictionary Create(uint size)
        {
            return new TypeSafeDictionary<T>(size);
        }
    }
}