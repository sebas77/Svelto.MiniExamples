namespace Svelto.Common.Internal
{
    public static class DebugExtensions
    {
        public static string TypeName<T>(this T any)
        {
            return TypeCache<T>.type.Name;
        }
    }
}