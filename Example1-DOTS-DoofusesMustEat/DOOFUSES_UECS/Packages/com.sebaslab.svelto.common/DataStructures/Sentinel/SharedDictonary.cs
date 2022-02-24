using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Unity.Burst;

namespace Svelto.Common.DataStructures
{
    public static class SharedDictonary
    {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        public static void InitFuncPtr()
        {
        }

        static SharedDictonary()
        {
            unsafe
            {
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
                SharedStatic<SharedDictionaryStruct> test = Unity.Burst.SharedStatic<SharedDictionaryStruct>
                   .GetOrCreate<SharedDictionaryStruct>();
#else
                    var test = default(SharedDictionaryStruct);
#endif
                test.Data.test = MemoryUtilities
                   .Alloc<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
                        SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((uint)1, Allocator.Persistent);

                Unsafe.AsRef<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
                        SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((void*)test.Data.test) =
                    new SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
                        SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>(1, Allocator.Persistent);

                SharedDictonary.test = test;
            }
        }
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        public static readonly Unity.Burst.SharedStatic<SharedDictionaryStruct> test;
#else
        public static readonly SharedDictionaryStruct test;
#endif
    }
}