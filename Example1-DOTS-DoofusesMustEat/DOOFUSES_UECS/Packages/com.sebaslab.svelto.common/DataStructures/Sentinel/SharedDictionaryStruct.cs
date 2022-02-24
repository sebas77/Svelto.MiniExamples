using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.Common.DataStructures
{
    public struct SharedDictionaryStruct
    {
        internal IntPtr test;

        SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
            SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>> cast
        {
            get
            {
                unsafe
                {
                    return Unsafe
                       .AsRef<SveltoDictionary<long, Sentinel, SentinelNativeStrategy<SveltoDictionaryNode<long>>,
                            SentinelNativeStrategy<Sentinel>, SentinelNativeStrategy<int>>>((void*)test);
                }
            }
        }


        public void Add(long ptr, Sentinel sentinel)
        {
            cast.Add(ptr, sentinel);
        }

        public ref Sentinel GetValueByRef(long ptr)
        {
            return ref cast.GetValueByRef(ptr);
        }

        public bool Exists(IntPtr ptr)
        {
            return cast.TryFindIndex((long)ptr, out _);
        }
    }
}