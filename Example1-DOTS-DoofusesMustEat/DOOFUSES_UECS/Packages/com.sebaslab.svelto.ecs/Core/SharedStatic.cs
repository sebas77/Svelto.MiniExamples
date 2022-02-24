namespace Svelto.ECS
{
    struct SharedStatic<T, Key> where T : unmanaged
    {
#if UNITY_BURST
        static readonly Unity.Burst.SharedStatic<T> uniqueContextID = Unity.Burst.SharedStatic<T>.GetOrCreate<Key>();

        public ref T Data => ref uniqueContextID.Data;
#endif

        public SharedStatic(T i)
        {
            Data = i;
        }
    }
}