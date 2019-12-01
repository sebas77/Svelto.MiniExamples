namespace Svelto.Common.Internal
{
    public static class DebugExtensions
    {
        public static string TypeName<T>(this T any)
        {
#if DEBUG && !PROFILER
            return GetName<T>.Name;
#else
            return "";
#endif
        }
    }

     public static class GetName<T>
     {
         static GetName()
         {
             Name = typeof(T).ToString();
         }
    
         public static string Name;
     }
}