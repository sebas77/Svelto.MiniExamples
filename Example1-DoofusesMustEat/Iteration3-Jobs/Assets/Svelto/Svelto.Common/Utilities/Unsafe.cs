namespace Svelto.Common
{
    public class UnsafeUtils
    {
        public static uint SizeOf<T>() where T : unmanaged
        {
            unsafe
            {
                return (uint) sizeof(T);
            }
        }
    }
}