namespace Svelto.Common
{
    //Note: SharedStatic MUST always be initialised outside burst otherwise undefined behaviour will happen
    public struct SharedStaticWrapper<T, Key> where T : unmanaged
    {
#if UNITY_BURST
        static readonly Unity.Burst.SharedStatic<T> uniqueContextID = Unity.Burst.SharedStatic<T>.GetOrCreate<Key>();

        public ref T Data => ref uniqueContextID.Data;
#else
        static T uniqueContextID;
        
        public ref T Data => ref uniqueContextID;
#endif

        public SharedStaticWrapper(T i)
        {
            Data = i;
        }
    }
}