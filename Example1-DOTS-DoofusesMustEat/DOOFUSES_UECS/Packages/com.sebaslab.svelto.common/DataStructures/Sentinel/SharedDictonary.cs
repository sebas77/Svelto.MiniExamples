using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.Common.DataStructures
{
    public static class SharedDictonary
    {
        public static void Init()
        {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#else
            var test = default(SharedDictionaryStruct);
#endif
            test.Data.test = SharedDictionaryStruct.Create();
        }

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        public static readonly Unity.Burst.SharedStatic<SharedDictionaryStruct> test =
            Unity.Burst.SharedStatic<SharedDictionaryStruct>.GetOrCreate<SharedDictionaryStruct>();
#else
        public static readonly SharedDictionaryStruct test;
#endif
    }
}