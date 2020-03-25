using Svelto.ECS;

namespace Svelto.Common
{
    public class UnsafeUtils
    {
        public static uint SizeOf<T>() where T : unmanaged, IEntityComponent
        {
            unsafe
            {
                return (uint) sizeof(T);
            }
        }
    }
}